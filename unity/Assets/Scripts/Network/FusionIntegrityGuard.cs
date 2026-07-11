using System;
using System.Collections.Generic;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public enum FusionIntegrityRejection
    {
        None = 0,
        InvalidSender = 1,
        InvalidCredential = 2,
        Replay = 3,
        RateLimited = 4,
        OutOfState = 5,
        InvalidRole = 6,
        InvalidPayload = 7,
        ImpossibleMovement = 8
    }

    public sealed class FusionIntegrityGuard
    {
        public const float MaximumMovementSpeed = 4.5f;
        public const float MovementTolerance = 0.85f;
        public const float MinimumMovementInterval = 0.01f;
        public const float ActionCooldown = 1.2f;
        public const float RestartCooldown = 1f;
        public const float BoundsTolerance = 0.35f;
        public const int MaximumCalledNameLength = 32;

        private readonly Dictionary<int, string> credentials = new Dictionary<int, string>();
        private readonly Dictionary<int, int> lastEnvelopeSequences = new Dictionary<int, int>();
        private readonly Dictionary<int, int> lastMovementSequences = new Dictionary<int, int>();
        private readonly Dictionary<int, int> lastActionSequences = new Dictionary<int, int>();
        private readonly Dictionary<int, float> lastMovementTimes = new Dictionary<int, float>();
        private readonly Dictionary<int, Vector2> lastMovementPositions = new Dictionary<int, Vector2>();
        private readonly Dictionary<string, float> lastActionTimes = new Dictionary<string, float>();
        private readonly Dictionary<int, float> lastRestartTimes = new Dictionary<int, float>();
        private readonly Dictionary<FusionIntegrityRejection, int> rejectionCounts =
            new Dictionary<FusionIntegrityRejection, int>();

        public int AcceptedCount { get; private set; }

        public int RejectedCount { get; private set; }

        public void Reset()
        {
            credentials.Clear();
            lastEnvelopeSequences.Clear();
            lastMovementSequences.Clear();
            lastActionSequences.Clear();
            lastMovementTimes.Clear();
            lastMovementPositions.Clear();
            lastActionTimes.Clear();
            lastRestartTimes.Clear();
            rejectionCounts.Clear();
            AcceptedCount = 0;
            RejectedCount = 0;
        }

        public void SetCredential(int playerIndex, string token)
        {
            if (playerIndex < 0 || string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            credentials[playerIndex] = token;
            lastEnvelopeSequences.Remove(playerIndex);
            lastMovementSequences.Remove(playerIndex);
            lastActionSequences.Remove(playerIndex);
            lastMovementTimes.Remove(playerIndex);
            lastMovementPositions.Remove(playerIndex);
            lastRestartTimes.Remove(playerIndex);
            RemoveActionTimesFor(playerIndex);
        }

        public void ResetMovementState()
        {
            lastMovementSequences.Clear();
            lastMovementTimes.Clear();
            lastMovementPositions.Clear();
        }

        public string CredentialFor(int playerIndex)
        {
            return credentials.TryGetValue(playerIndex, out var token) ? token : string.Empty;
        }

        public bool ValidateEnvelope(
            FusionNetworkEnvelope envelope,
            int rosterSize,
            out FusionIntegrityRejection rejection)
        {
            if (envelope == null || envelope.senderIndex < 0 || envelope.senderIndex >= rosterSize)
            {
                return Reject(FusionIntegrityRejection.InvalidSender, out rejection);
            }

            if (!credentials.TryGetValue(envelope.senderIndex, out var expectedToken)
                || !ConstantTimeEquals(expectedToken, envelope.authorityToken))
            {
                return Reject(FusionIntegrityRejection.InvalidCredential, out rejection);
            }

            if (lastEnvelopeSequences.TryGetValue(envelope.senderIndex, out var lastSequence)
                && envelope.sequence <= lastSequence)
            {
                return Reject(FusionIntegrityRejection.Replay, out rejection);
            }

            lastEnvelopeSequences[envelope.senderIndex] = envelope.sequence;
            rejection = FusionIntegrityRejection.None;
            return true;
        }

        public bool ValidateMovement(
            int senderIndex,
            FusionMovementPayload payload,
            Bounds mapBounds,
            float authorityTime,
            bool roundRunning,
            out FusionIntegrityRejection rejection)
        {
            if (!roundRunning)
            {
                return Reject(FusionIntegrityRejection.OutOfState, out rejection);
            }

            if (payload == null
                || payload.networkPlayerId != NetworkPlayerIdFor(senderIndex)
                || payload.sequence <= 0
                || !IsFinite(payload.x)
                || !IsFinite(payload.y)
                || !IsFinite(payload.inputX)
                || !IsFinite(payload.inputY)
                || !IsFinite(payload.sentAt)
                || !Enum.IsDefined(typeof(PlayerFacingDirection), payload.facingDirection)
                || new Vector2(payload.inputX, payload.inputY).sqrMagnitude > 1.05f * 1.05f)
            {
                return Reject(FusionIntegrityRejection.InvalidPayload, out rejection);
            }

            if (lastMovementSequences.TryGetValue(senderIndex, out var lastSequence)
                && payload.sequence <= lastSequence)
            {
                return Reject(FusionIntegrityRejection.Replay, out rejection);
            }

            var position = new Vector2(payload.x, payload.y);
            var expandedBounds = mapBounds;
            expandedBounds.Expand(new Vector3(BoundsTolerance * 2f, BoundsTolerance * 2f, 0f));
            if (!expandedBounds.Contains(new Vector3(position.x, position.y, expandedBounds.center.z)))
            {
                return Reject(FusionIntegrityRejection.ImpossibleMovement, out rejection);
            }

            if (lastMovementTimes.TryGetValue(senderIndex, out var lastTime))
            {
                var elapsed = Mathf.Max(0f, authorityTime - lastTime);
                if (elapsed < MinimumMovementInterval)
                {
                    return Reject(FusionIntegrityRejection.RateLimited, out rejection);
                }

                var maximumDistance = MaximumMovementSpeed * elapsed + MovementTolerance;
                if (lastMovementPositions.TryGetValue(senderIndex, out var lastPosition)
                    && Vector2.Distance(lastPosition, position) > maximumDistance)
                {
                    return Reject(FusionIntegrityRejection.ImpossibleMovement, out rejection);
                }
            }

            lastMovementSequences[senderIndex] = payload.sequence;
            lastMovementTimes[senderIndex] = authorityTime;
            lastMovementPositions[senderIndex] = position;
            return Accept(out rejection);
        }

        public bool ValidateAction(
            int senderIndex,
            FusionActionPayload payload,
            float authorityTime,
            bool roundRunning,
            out FusionIntegrityRejection rejection)
        {
            if (!roundRunning)
            {
                return Reject(FusionIntegrityRejection.OutOfState, out rejection);
            }

            if (payload == null
                || payload.actorNetworkPlayerId != NetworkPlayerIdFor(senderIndex)
                || payload.sequence <= 0
                || !Enum.IsDefined(typeof(PrototypeNetworkActionKind), payload.kind)
                || !Enum.IsDefined(typeof(PlayerFacingDirection), payload.facingDirection)
                || payload.calledName == null
                || payload.calledName.Length > MaximumCalledNameLength)
            {
                return Reject(FusionIntegrityRejection.InvalidPayload, out rejection);
            }

            var kind = (PrototypeNetworkActionKind)payload.kind;
            if (kind == PrototypeNetworkActionKind.BangNameCall
                && string.IsNullOrWhiteSpace(PlayerNameIdentity.NormalizeName(payload.calledName)))
            {
                return Reject(FusionIntegrityRejection.InvalidPayload, out rejection);
            }

            if ((senderIndex == 0 && kind != PrototypeNetworkActionKind.BangNameCall)
                || (senderIndex != 0 && kind != PrototypeNetworkActionKind.SakCounter))
            {
                return Reject(FusionIntegrityRejection.InvalidRole, out rejection);
            }

            if (lastActionSequences.TryGetValue(senderIndex, out var lastSequence)
                && payload.sequence <= lastSequence)
            {
                return Reject(FusionIntegrityRejection.Replay, out rejection);
            }

            var actionKey = BuildActionKey(senderIndex, kind, payload.calledName);
            if (lastActionTimes.TryGetValue(actionKey, out var lastActionTime)
                && authorityTime - lastActionTime < ActionCooldown)
            {
                return Reject(FusionIntegrityRejection.RateLimited, out rejection);
            }

            lastActionSequences[senderIndex] = payload.sequence;
            lastActionTimes[actionKey] = authorityTime;
            return Accept(out rejection);
        }

        public bool ValidateRestart(
            int senderIndex,
            float authorityTime,
            bool roundFinished,
            out FusionIntegrityRejection rejection)
        {
            if (!roundFinished)
            {
                return Reject(FusionIntegrityRejection.OutOfState, out rejection);
            }

            if (lastRestartTimes.TryGetValue(senderIndex, out var lastRestartTime)
                && authorityTime - lastRestartTime < RestartCooldown)
            {
                return Reject(FusionIntegrityRejection.RateLimited, out rejection);
            }

            lastRestartTimes[senderIndex] = authorityTime;
            return Accept(out rejection);
        }

        public int RejectionCount(FusionIntegrityRejection rejection)
        {
            return rejectionCounts.TryGetValue(rejection, out var count) ? count : 0;
        }

        public static string NetworkPlayerIdFor(int playerIndex)
        {
            return $"preview-{Mathf.Max(0, playerIndex):00}";
        }

        private bool Accept(out FusionIntegrityRejection rejection)
        {
            AcceptedCount += 1;
            rejection = FusionIntegrityRejection.None;
            return true;
        }

        private bool Reject(FusionIntegrityRejection value, out FusionIntegrityRejection rejection)
        {
            RejectedCount += 1;
            rejectionCounts[value] = RejectionCount(value) + 1;
            rejection = value;
            return false;
        }

        private void RemoveActionTimesFor(int playerIndex)
        {
            var prefix = playerIndex + ":";
            var keys = new List<string>();
            foreach (var pair in lastActionTimes)
            {
                if (pair.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    keys.Add(pair.Key);
                }
            }

            for (var index = 0; index < keys.Count; index += 1)
            {
                lastActionTimes.Remove(keys[index]);
            }
        }

        private static string BuildActionKey(
            int senderIndex,
            PrototypeNetworkActionKind kind,
            string calledName)
        {
            var target = kind == PrototypeNetworkActionKind.BangNameCall
                ? PlayerNameIdentity.NormalizeName(calledName)
                : string.Empty;
            return $"{senderIndex}:{(int)kind}:{target}";
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool ConstantTimeEquals(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index += 1)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }
}

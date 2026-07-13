using System;
using System.Text;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public enum FusionNetworkMessageKind
    {
        AuthorityGrant = 1,
        MovementRequest = 3,
        ActionRequest = 4,
        RoundState = 5,
        RestartRequest = 6,
        MovementState = 7,
        ActionState = 8,
        ResumeStateRequest = 9
    }

    [Serializable]
    public sealed class FusionNetworkEnvelope
    {
        public int protocolVersion;
        public int kind;
        public int senderIndex;
        public int sequence;
        public string authorityToken;
        public string payload;
    }

    [Serializable]
    public sealed class FusionAuthorityGrantPayload
    {
        public int playerIndex;
        public int authorityEpoch;
        public string authorityToken;
    }

    [Serializable]
    public sealed class FusionCommandPayload
    {
        public string command;
        public string requestId;
    }

    [Serializable]
    public sealed class FusionMovementPayload
    {
        public string networkPlayerId;
        public float x;
        public float y;
        public float inputX;
        public float inputY;
        public int facingDirection;
        public int sequence;
        public float sentAt;
    }

    [Serializable]
    public sealed class FusionActionPayload
    {
        public int kind;
        public int outcome;
        public string actorNetworkPlayerId;
        public string targetNetworkPlayerId;
        public string calledName;
        public string targetDisplayName;
        public float originX;
        public float originY;
        public float pointX;
        public float pointY;
        public float directionX;
        public float directionY;
        public int facingDirection;
        public int sequence;
        public float sentAt;
    }

    [Serializable]
    public sealed class FusionRoundPayload
    {
        public int state;
        public int result;
        public string resultTitle;
        public string resultMessage;
        public int totalHiders;
        public int remainingHiders;
        public float remainingSeconds;
        public int roundNumber;
        public int caughtPlayerMask;
        public bool tayaCountered;
        public string authorityRoundId;
        public string resumeRequestId;
    }

    public static class FusionNetworkProtocol
    {
        public const int Version = 2;
        public const int MaximumPayloadBytes = 16 * 1024;
        public const int MaximumAuthorityTokenLength = 64;

        public static byte[] Encode<T>(
            FusionNetworkMessageKind kind,
            int senderIndex,
            int sequence,
            T payload,
            string authorityToken = "")
        {
            var envelope = new FusionNetworkEnvelope
            {
                protocolVersion = Version,
                kind = (int)kind,
                senderIndex = senderIndex,
                sequence = sequence,
                authorityToken = authorityToken ?? string.Empty,
                payload = payload == null ? string.Empty : JsonUtility.ToJson(payload)
            };

            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(envelope));
        }

        public static bool TryDecode(byte[] data, out FusionNetworkEnvelope envelope)
        {
            envelope = null;
            if (data == null || data.Length == 0 || data.Length > MaximumPayloadBytes)
            {
                return false;
            }

            try
            {
                envelope = JsonUtility.FromJson<FusionNetworkEnvelope>(Encoding.UTF8.GetString(data));
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (envelope == null
                || envelope.protocolVersion != Version
                || !Enum.IsDefined(typeof(FusionNetworkMessageKind), envelope.kind)
                || envelope.senderIndex < 0
                || envelope.sequence <= 0
                || envelope.authorityToken == null
                || envelope.authorityToken.Length > MaximumAuthorityTokenLength
                || envelope.payload == null)
            {
                envelope = null;
                return false;
            }

            return true;
        }

        public static bool TryDecodePayload<T>(FusionNetworkEnvelope envelope, out T payload)
            where T : class
        {
            payload = null;
            if (envelope == null || string.IsNullOrWhiteSpace(envelope.payload))
            {
                return false;
            }

            try
            {
                payload = JsonUtility.FromJson<T>(envelope.payload);
                return payload != null;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}

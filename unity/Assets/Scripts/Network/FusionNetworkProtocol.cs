using System;
using System.Text;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public enum FusionNetworkMessageKind
    {
        Movement = 3,
        Action = 4,
        RoundState = 5,
        RestartRequest = 6
    }

    [Serializable]
    public sealed class FusionNetworkEnvelope
    {
        public int protocolVersion;
        public int kind;
        public int senderIndex;
        public int sequence;
        public string payload;
    }

    [Serializable]
    public sealed class FusionCommandPayload
    {
        public string command;
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
    }

    public static class FusionNetworkProtocol
    {
        public const int Version = 1;
        public const int MaximumPayloadBytes = 16 * 1024;

        public static byte[] Encode<T>(
            FusionNetworkMessageKind kind,
            int senderIndex,
            int sequence,
            T payload)
        {
            var envelope = new FusionNetworkEnvelope
            {
                protocolVersion = Version,
                kind = (int)kind,
                senderIndex = senderIndex,
                sequence = sequence,
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

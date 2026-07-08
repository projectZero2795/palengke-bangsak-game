using System;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PlayerNameIdentity : MonoBehaviour
    {
        [SerializeField]
        private string displayName = "Player";

        public string DisplayName
        {
            get
            {
                var trimmedName = (displayName ?? string.Empty).Trim();
                return string.IsNullOrEmpty(trimmedName) ? gameObject.name : trimmedName;
            }
        }

        public void SetDisplayName(string newDisplayName)
        {
            displayName = newDisplayName;
        }

        public bool MatchesCalledName(string calledName)
        {
            return string.Equals(
                NormalizeName(DisplayName),
                NormalizeName(calledName),
                StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Trim().Split(
                new[] { ' ', '\t', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

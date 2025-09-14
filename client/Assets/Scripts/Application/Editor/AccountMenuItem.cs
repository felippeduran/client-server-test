using UnityEngine;
using UnityEditor;

namespace Application.Editor
{
    public static class AccountMenuItem
    {
        [MenuItem("Account/Clean All")]
        static void CleanAccount()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}

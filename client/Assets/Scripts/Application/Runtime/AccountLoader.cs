using UnityEngine;
using System;

namespace Application.Runtime
{
    public class AccountLoader
    {
        public Account Load()
        {
            var account = new Account
            {
                Id = PlayerPrefs.GetString("AccountId"),
                AccessToken = PlayerPrefs.GetString("AccessToken"),
            };
            if (string.IsNullOrEmpty(account.Id) || string.IsNullOrEmpty(account.AccessToken))
            {
                account = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    AccessToken = Guid.NewGuid().ToString(),
                };

                PlayerPrefs.SetString("AccountId", account.Id);
                PlayerPrefs.SetString("AccessToken", account.AccessToken);
            }

            return account;
        }
    }
}
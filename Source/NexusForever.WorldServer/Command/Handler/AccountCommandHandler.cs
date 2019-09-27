﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Database.Auth;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.GameTable.Static;
using NexusForever.Shared.Database.Auth.Model;
using NexusForever.Shared.Game.Events;
using NexusForever.WorldServer.Command.Attributes;
using NexusForever.WorldServer.Command.Contexts;
using NexusForever.WorldServer.Game.Account.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Command.Handler
{
    [Name("Account Management", Permission.None)]
    public class AccountCommandHandler : CommandCategory
    {
        public AccountCommandHandler()
            : base(false, "acc", "account")
        {
        }

        [SubCommandHandler("create", "email password [extraRoles] - Create a new account", Permission.CommandAccountCreate)]
        public async Task HandleAccountCreate(CommandContext context, string subCommand, string[] parameters, IEnumerable<ChatFormat> chatLinks)
        {
            if (parameters.Length != 2)
            {
                await SendHelpAsync(context).ConfigureAwait(false);
                return;
            }

            List<ulong> extraRoles = new List<ulong>();
            for (int i = 2; i < parameters.Length; i++)
                extraRoles.Add(ulong.Parse(parameters[i]));

            AuthDatabase.CreateAccount(parameters[0], parameters[1], defaultRole: ConfigurationManager<WorldServerConfiguration>.Config.DefaultRole, extraRoles.ToArray());
            await context.SendMessageAsync($"Account {parameters[0]} created successfully")
                .ConfigureAwait(false);
        }

        [SubCommandHandler("delete", "email - Delete an account", Permission.CommandAccountDelete)]
        public async Task HandleAccountDeleteAsync(CommandContext context, string subCommand, string[] parameters, IEnumerable<ChatFormat> chatLinks)
        {
            if (parameters.Length < 1)
            {
                await SendHelpAsync(context).ConfigureAwait(false);
                return;
            }

            if (AuthDatabase.DeleteAccount(parameters[0]))
                await context.SendMessageAsync($"Account {parameters[0]} successfully removed!")
                    .ConfigureAwait(false);
            else
                await context.SendMessageAsync($"Cannot find account with Email: {parameters[0]}")
                    .ConfigureAwait(false);
        }

        [SubCommandHandler("currencyadd", "currencyId amount - Add the amount provided to the currencyId provided")]
        public Task HandleAccountCurrencyAdd(CommandContext context, string command, string[] parameters)
        {
            if (context.Session.Account == null)
            {
                context.SendMessageAsync("Account not found. Please try again.");
                return Task.CompletedTask;
            }

            if (parameters.Length != 2)
            {
                context.SendMessageAsync("Parameters are invalid. Please try again.");
                return Task.CompletedTask;
            }

            bool currencyParsed = byte.TryParse(parameters[0], out byte currencyId);
            if (!currencyParsed || currencyId == 13) // Disabled Character Token for now due to causing server errors if the player tries to use it. TODO: Fix level 50 creation
            {
                context.SendMessageAsync("Invalid currencyId. Please try again.");
                return Task.CompletedTask;
            }

            AccountCurrencyTypeEntry currencyEntry = GameTableManager.AccountCurrencyType.GetEntry(currencyId);
            if (currencyEntry == null)
            {
                context.SendMessageAsync("Invalid currencyId. Please try again.");
                return Task.CompletedTask;
            }

            if (!uint.TryParse(parameters[1], out uint amount))
            {
                context.SendMessageAsync("Unable to parse amount. Please try again.");
                return Task.CompletedTask;
            }

            context.Session.AccountCurrencyManager.CurrencyAddAmount((AccountCurrencyType)currencyId, amount);
            return Task.CompletedTask;
        }

        [SubCommandHandler("currencylist", "List all account currencies")]
        public Task handleAccountCurrencyList(CommandContext context, string command, string[] parameters)
        {
            var tt = GameTableManager.GetTextTable(Language.English);
            foreach (var entry in GameTableManager.AccountCurrencyType.Entries)
            {
                context.SendMessageAsync($"ID {entry.Id}: {tt.GetEntry(entry.LocalizedTextId)}");
            }

            return Task.CompletedTask;
        }
    }
}

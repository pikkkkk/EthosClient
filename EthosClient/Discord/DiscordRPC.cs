﻿using EthosClient.Settings;
using EthosClient.Utils;
using EthosClient.Wrappers;
using Il2CppSystem.Threading.Tasks;
using MelonLoader.ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Timers;
using VRC.Core;
using static EthosClient.Discord.DiscordRpc;

namespace EthosClient.Discord
{
    public static class DiscordRPC
    {
        private static RichPresence presence;
        private static EventHandlers eventHandlers;
        private static bool IsStarted = false;

        public static void Start()
        {
            Directory.CreateDirectory("Dependencies");
            new System.Threading.Thread(async () =>
            {
                if (!File.Exists("Dependencies/discord-rpc.dll"))
                {
                    //HTTP CLIENT BECAUSE WEB CLIENTS ARE FUCKING STUPID *COUGH BAD YAEKITH*
                    var bytes = await new HttpClient().GetByteArrayAsync("http://yaekiths-projects.xyz/discord-rpc.dll"); //lets get it from somewhere reliable LOL
                    // Added await to avoid errors.
                    File.WriteAllBytes("Dependencies/discord-rpc.dll", bytes);
                    Thread.Sleep(5000);
                    ConsoleUtil.Info("[DEBUG] Downloaded Discord-rpc.dll");
                }
                ConsoleUtil.Info("[DEBUG] Started Rich Presence.");
                eventHandlers = default;
                presence.details = "A very cool public free cheat";
                presence.state = "Starting Game...";
                presence.largeImageKey = "ethos_logo"; // YAEKITH STOP TOUCHING DISCORD RPC
                presence.smallImageKey = "small_ethos";
                presence.partySize = 0;
                presence.partyMax = 0;
                presence.startTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                StartClient(); 
                new Thread(() =>
                {
                    System.Timers.Timer timer = new System.Timers.Timer(15000.0);
                    timer.Elapsed += Update;
                    timer.AutoReset = true;
                    timer.Enabled = true;
                }).Start();
            }).Start();
            
        }
        public static void StartClient()
        {
            if (!IsStarted)
            {
                Initialize("735902136629592165", ref eventHandlers, true, "");
                IsStarted = true;
            }
            if (Configuration.GetConfig().UseRichPresence) UpdatePresence(ref presence);
            else
            {
                Shutdown();
                IsStarted = false;
            }
        }

        public static void Update(object sender, ElapsedEventArgs args)
        {
            if (Configuration.GetConfig().UseRichPresence)
            {
                if (APIUser.CurrentUser == null)
                {
                    eventHandlers = default(EventHandlers);
                    presence.details = "A very cool public free cheat";
                    presence.state = "Starting Game...";
                    presence.largeImageKey = "ethos_logo";
                    presence.smallImageKey = "small_ethos";
                    presence.partySize = 0;
                    presence.partyMax = 0;
                    presence.largeImageKey = "Funeral Client V2";
                    presence.smallImageText = GeneralUtils.Version;
                    UpdatePresence(ref presence);
                    return;
                }
                var room = RoomManagerBase.field_Internal_Static_ApiWorld_0;
                if (room != null)
                {
                    presence.partySize = 1;
                    presence.partyMax = GeneralWrappers.GetPlayerManager().GetAllPlayers().Length;
                    switch (room.currentInstanceAccess)
                    {
                        default:
                            presence.state = $"Transitioning to another Instance";
                            presence.partySize = 0;
                            presence.partyMax = 0;
                            presence.largeImageKey = "big_pog";
                            presence.smallImageKey = "ethos_logo";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.Counter:
                            presence.state = $"In a Counter Instance";
                            presence.smallImageKey = "ethos_logo";
                            presence.largeImageKey = "small_ethos";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.InviteOnly:
                            presence.state = "In an Invite Only Instance";
                            presence.largeImageKey = "even_more_pog";
                            presence.smallImageKey = "small_ethos";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.InvitePlus:
                            presence.state = "In an Invite+ Instance";
                            presence.largeImageKey = "even_more_pog";
                            presence.smallImageKey = "small_ethos";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.Public:
                            presence.state = "In a Public Instance";
                            presence.largeImageKey = "ethos_logo";
                            presence.smallImageKey = "small_ethos";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.FriendsOfGuests:
                            presence.state = "In a Friends Of Guests Instance";
                            presence.largeImageKey = "ethos_logo";
                            presence.smallImageKey = "funeral_logo";
                            break;
                        case VRC.Core.ApiWorldInstance.AccessType.FriendsOnly:
                            presence.state = "In a Friends Only Instance";
                            presence.largeImageKey = "ethos_logo";
                            presence.smallImageKey = "small_ethos";
                            break;
                    }
                }
                else
                {
                    presence.state = $"Transitioning to another Instance";
                    presence.partySize = 0;
                    presence.partyMax = 0;
                    presence.largeImageKey = "ethos_logo";
                    presence.smallImageKey = "small_ethos";
                }
                presence.largeImageText = $"As {((APIUser.CurrentUser != null) ? APIUser.CurrentUser.displayName : "")} {(GeneralUtils.IsDevBranch ? "(Developer)" : "(User)")} [{(!VRCTrackingManager.Method_Public_Static_Boolean_9() ? "VR" : "Desktop")}]";
                presence.smallImageText = GeneralUtils.Version;
                UpdatePresence(ref presence);
            }
        }
    }
}

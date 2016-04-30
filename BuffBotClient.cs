/*
 Copyright (c) 2012-2013 Clint Banzhaf
 This file is part of "Meridian59 .NET".

 "Meridian59 .NET" is free software: 
 You can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, 
 either version 3 of the License, or (at your option) any later version.

 "Meridian59 .NET" is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 See the GNU General Public License for more details.

 You should have received a copy of the GNU General Public License along with "Meridian59 .NET".
 If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using Meridian59.Common;
using Meridian59.Common.Enums;
using Meridian59.Data.Models;
using Meridian59.Data.Lists;
using Meridian59.Protocol.GameMessages;
using Meridian59.Data;
using Meridian59.Files;
using Meridian59.Common.Constants;

namespace Meridian59.Bot.Buff
{
    /// <summary>
    /// A client which acts as a Buff bot
    /// </summary>
    public class BuffBotClient : BotClient<GameTick, ResourceManager, DataController, BuffBotConfig>
    {
        #region Constants
        protected const string NAME_SHILLING			= "shilling";
        protected const string COMMAND_BUFF				= "buff";
        protected const string COMMAND_GIVECASH			= "givecash";
        protected const string TELL_THANKS				= "Thank you for business.";
        protected const string LOG_ADVBROADCASTED		= "Advertisement broadcasted";
        #endregion

        protected bool isGiveCashOffer = false;
        protected bool isMeditateInProcess = false;
        protected bool isBuffingInProcess = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuffBotClient()
            : base()
        {                       

        }

        /// <summary>
        /// 
        /// </summary>
        public override void Init()
        {
            base.Init();

            // set intervals
            GameTick.INTERVALBROADCAST = Config.IntervalBroadcast;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        protected override void HandleGameModeMessage(GameModeMessage Message)
        {
            base.HandleGameModeMessage(Message);           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Message"></param>
        protected override void HandlePlayerMessage(PlayerMessage Message)
        {
            base.HandlePlayerMessage(Message);

            // make sure we're resting to have mana for broadcasts
            SendUserCommandRest();  
        }

        /// <summary>
        /// Handles someone offers you first
        /// </summary>
        /// <param name="Message"></param>
        protected override void HandleOfferMessage(OfferMessage Message)
        {
            base.HandleOfferMessage(Message);
            
            // no tradepartner set (bug!?) or
            // someone tried to offer nothing (pointless)
            if (Data.Trade.TradePartner == null || Data.Trade.ItemsPartner.Count == 0)
            {
                SendCancelOffer();
                return;
            }

            ///
            /// ADMIN
            ///
            
            // accept any items from configured admins in config, offer nothing in return
            if (Config.IsAdmin(Data.Trade.TradePartner.Name))
            {
                // nothing
                SendReqCounterOffer(new ObjectID[0]);

                // tell admin
                SendSayGroupMessage(
                    Data.Trade.TradePartner.ID,
                    Config.ChatPrefixString + "I will take that, master " + Data.Trade.TradePartner.Name);

                // exit
                return;
            }

            ///
            /// NORMAL
            ///
			
            // see what they offered
            for(int i = 0; i < Data.Trade.ItemsPartner.Count; i++)
            {
                ObjectBase obj = Data.Trade.ItemsPartner[i];

                // perform action on shillings
                if (!Config.IsAdmin(Data.Trade.TradePartner.Name)) {
                    if (obj.Name == NAME_SHILLING || obj.Name == "mushroom" || obj.Name == "red mushroom"
                        || obj.Name == "purple mushroom" || obj.Name == "blue dragon scale" || obj.Name == "sapphire"
                        || obj.Name == "orc tooth" || obj.Name == "elderberry" || obj.Name == "kriipa claw" || obj.Name == "vial of solagh"
                        || obj.Name == "Inky-cap mushroom") {
                        if (Config.Enabledonations == true)
                        {
                            // offer nothing
                            SendReqCounterOffer(new ObjectID[0]);

                            return;
                        }
                    }
                }

                else {
                    SendCancelOffer();
                }
			}
        }

        protected void PerformBuff(RoomObject roomObject, string partnername) {
            isBuffingInProcess = true;

            string[] spellName = { "bless", "resist poison", "super strength", "detect invisible", "free action", "magic shield", "night vision", "deflect", "eagle eyes",
                                        "armor of gort", "meditate"};

            for (int i = 0; i < spellName.Length; i++)
            {
                SpellObject spellObject = null;
                StatList spellStat = null;
                ReqCastMessage reqCastMsg = null;

                spellObject = Data.SpellObjects.GetItemByName(spellName[i], false);
                if (spellObject != null) { spellStat = Data.AvatarSpells.GetItemByID(spellObject.ID); }

                if (spellObject == null || spellStat == null)
                {
                    Log("WARN", "Can't find spell " + spellObject.Name + ".");
                    return;
                }

                if (roomObject == null)
                {
                    Log("WARN", "Can't find RoomObject of " + partnername + ".");
                    return;
                }

                if (spellObject.Name == "meditate")
                {
                    if (Data.IsResting) {
                        SendUserCommandStand();
                    }

                    isMeditateInProcess = true;
                    SendReqCastMessage(spellObject);
                    System.Threading.Thread.Sleep(21000);
                    isMeditateInProcess = false;
                    isBuffingInProcess = false;

                    if (!Data.IsResting) {
                        SendUserCommandRest();
                    }

                    return;
                }

                else if (spellObject.Name == "deflect")
                {
                    if (Data.IsResting) {
                        SendUserCommandStand();
                    }

                    reqCastMsg = new ReqCastMessage(spellObject.ID, new ObjectID[] { new ObjectID(roomObject.ID) });
                    SendGameMessage(reqCastMsg);

                    Log("BOT", "I casted spell " + spellObject.Name + " on target " + partnername + ".");
                    System.Threading.Thread.Sleep(3000);
                }

                else if (spellObject.Name != "deflect" && spellObject.Name != "meditate")
                {
                    if (Data.IsResting) {
                        SendUserCommandStand();
                    }

                    reqCastMsg = new ReqCastMessage(spellObject.ID, new ObjectID[] { new ObjectID(roomObject.ID) });
                    SendGameMessage(reqCastMsg);

                    Log("BOT", "I casted spell " + spellObject.Name + " on target " + partnername + ".");
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }

        protected void UseInky(InventoryObject inventoryObject) {
            if (inventoryObject == null) { Log("WARN", "Can't find item " + inventoryObject.Name + " in inventory."); return; }

            if (!inventoryObject.IsInUse) SendReqUseMessage(inventoryObject.ID);
            else SendReqUnuseMessage(inventoryObject.ID);

            Log("BOT", "Used item " + inventoryObject.Name + ".");
            System.Threading.Thread.Sleep(10000);
        }

        protected void CastMeditate() {
            isMeditateInProcess = true;
            SpellObject spellObject = null;
            StatList spellStat = null;

            spellObject = Data.SpellObjects.GetItemByName("meditate", false);
            if (spellObject != null) { spellStat = Data.AvatarSpells.GetItemByID(spellObject.ID); }

            if (spellObject == null || spellStat == null)
            {
                Log("WARN", "Can't find spell " + spellObject.Name + ".");
                return;
            }

            if (Data.IsResting)
            {
                SendUserCommandStand();
            }

            SendReqCastMessage(spellObject);
            System.Threading.Thread.Sleep(21000);
            isMeditateInProcess = false;

            if (!Data.IsResting)
            {
                SendUserCommandRest();
            }
        }

        /// <summary>
        /// Handles a counteroffer (you offered first)
        /// </summary>
        /// <param name="Message"></param>
        protected override void HandleCounterOfferMessage(CounterOfferMessage Message)
        {
            base.HandleCounterOfferMessage(Message);

            if (Data.Trade.TradePartner == null)
                return;

            // accept anything from configured admins in config
            if (Config.IsAdmin(Data.Trade.TradePartner.Name))
            {
                // tell admin
                SendSayGroupMessage(
                    new uint[] { Data.Trade.TradePartner.ID },
                    Config.ChatPrefixString + "Thank you, master " + Data.Trade.TradePartner.Name);

                // preserve partnername (gets cleaned next call)
                string partnername = String.Copy(Data.Trade.TradePartner.Name);

                // do trade
                SendAcceptOffer();

                if (!isGiveCashOffer)
                {
                    RoomObject roomObject = Data.RoomObjects.GetItemByName(partnername, false);

                    Thread BuffThread = new Thread(delegate() { PerformBuff(roomObject, partnername); });
                    BuffThread.Start();
                } else {
                    isGiveCashOffer = false;
                    return;
                }

                // exit
                return;
            }

            uint offeredsum = 0;
            uint expectedsum = Config.Buffprice;

            // get how much he offers us
            foreach (ObjectBase obj in Data.Trade.ItemsPartner)
                if (obj.Name == NAME_SHILLING)
                    offeredsum += obj.Count;

            // if he offered enough, accept
            if (offeredsum >= expectedsum)
            {
                // tell customer
                SendSayGroupMessage(
                    Data.Trade.TradePartner.ID, 
                    Config.ChatPrefixString + TELL_THANKS);

                // preserve partnername (gets cleaned next call)
                string partnername = String.Copy(Data.Trade.TradePartner.Name);

                // do trade
                SendAcceptOffer();

                Log("GOOD", "I sold buffs to " + partnername + " for " + offeredsum + " shillings.");

                RoomObject roomObject = Data.RoomObjects.GetItemByName(partnername, false);

                Thread BuffThread = new Thread(delegate () { PerformBuff(roomObject, partnername); });
                BuffThread.Start();
            }
            else
            {
                // tell customer
                SendSayGroupMessage(
                    Data.Trade.TradePartner.ID, 
                    Config.ChatPrefixString + "Sorry, I was expecting " + expectedsum + " shillings from you.");

                SendCancelOffer();
            }
        }

        /// <summary>
        /// Handles a new player enters room
        /// </summary>
        /// <param name="Message"></param>
        protected override void HandleCreateMessage(CreateMessage Message)
        {
            base.HandleCreateMessage(Message);

            RoomObject roomObject = Message.NewRoomObject;

            // tell offers to new roomobject
            if (roomObject.Flags.IsPlayer &&
                !roomObject.IsAvatar &&
                Config.TellOnEnter)
            {
                TellAdvertisement(roomObject);
            }

            // check if is bot admin who entered our room
            if (Config.IsAdmin(roomObject.Name))
            {
                // get our shillings from inventory
                InventoryObject shillings = Data.InventoryObjects.GetItemByName(NAME_SHILLING, false);

                uint count = 0;

                // tell master shillingscount
                if (shillings != null)
                    count = shillings.Count;

                SendSayGroupMessage(
                    roomObject.ID,
                    Config.ChatPrefixString + "Master " + roomObject.Name + ", I have " + count + " shillings");

                // getting list of missing reagents
                string[] reagentName = { "mushroom", "red mushroom", "purple mushroom", "blue dragon scale", "sapphire",
                "orc tooth", "elderberry", "herb", "kriipa claw", "vial of solagh", "Inky-cap mushroom" };
                string missingReagents = null;

                for (int i = 0; i < reagentName.Length; i++)
                {
                    InventoryObject inventoryObject = Data.InventoryObjects.GetItemByName(reagentName[i], false);

                    if (inventoryObject == null || inventoryObject.Count == 1)
                    {
                        if (missingReagents == null)
                        {
                            missingReagents += reagentName[i];
                        }
                        else
                        {
                            missingReagents += ", " + reagentName[i];
                        }
                    }
                }

                if (missingReagents != null)
                {
                    // notice about reagent miss
                    SendSayGroupMessage(roomObject.ID, Config.ChatPrefixString + "The bot is missing following reagents at the moment: " + missingReagents + ".");
                }
            }
        }

        /// <summary>
        /// Main handler for commands
        /// </summary>
        /// <param name="PartnerID"></param>
        /// <param name="Words">First element is command name</param>
        protected override void ProcessCommand(uint PartnerID, string[] Words)
        {
            switch (Words[0])
            {
                case COMMAND_BUFF:
                    ProcessCommandBuff(PartnerID, Words);
                    break;

                case COMMAND_GIVECASH:
                    ProcessCommandGiveCash(PartnerID, Words);
                    break;

                default:
                    // show help
                    SendSayGroupMessage(PartnerID, GetHelp());
                    break;
            }           
        }

        /// <summary>
        /// Processed a received buff command
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <param name="Words"></param>
        protected void ProcessCommandBuff(uint CustomerID, string[] Words)
        {
            if (!isMeditateInProcess)
            {
                // get the object we trade with
                RoomObject tradePartner = Data.RoomObjects.GetItemByID(CustomerID);

                if (tradePartner == null)
                    return;

                // save this as our tradepartner (required for we offer first)
                Data.Trade.TradePartner = tradePartner;

                // how much we expect for our offer
                uint expectedsum = Config.Buffprice;

                // finally offer him stackable item
                SendReqOffer(new ObjectID(CustomerID), new ObjectID[0]);

                // tell customer
                SendSayGroupMessage(CustomerID, Config.ChatPrefixString + "This costs you " + expectedsum + " shillings. Please, verify that you didn't attacked someone else too recently and you are not red (murderer).");

                // getting list of missing reagents
                string[] reagentName = { "mushroom", "red mushroom", "purple mushroom", "blue dragon scale", "sapphire",
                "orc tooth", "elderberry", "herb", "kriipa claw", "vial of solagh", "Inky-cap mushroom" };
                string missingReagents = null;

                for (int i = 0; i < reagentName.Length; i++)
                {
                    InventoryObject inventoryObject = Data.InventoryObjects.GetItemByName(reagentName[i], false);

                    if (inventoryObject == null)
                    {
                        if (missingReagents == null)
                        {
                            missingReagents += reagentName[i];
                        }
                        else
                        {
                            missingReagents += ", " + reagentName[i];
                        }
                    }
                }

                if (missingReagents != null)
                {
                    // notice about reagent miss
                    SendSayGroupMessage(CustomerID, Config.ChatPrefixString + "The bot is missing following reagents at the moment: " + missingReagents + ". If you have those reagents, simply offer them to the bot with your money.");
                }
            } else {
                SendSayGroupMessage(CustomerID, Config.ChatPrefixString + "Meditate is in process at the moment. Please, try again later.");
            }
        }

        /// <summary>
        /// Processed an received botadmin command to give shillings
        /// </summary>
        /// <param name="AdminID"></param>
        protected void ProcessCommandGiveCash(uint AdminID, string[] Words)
        {
            // try to get player from who list since it's a whisper
            RoomObject roomObject = Data.RoomObjects.GetItemByID(AdminID);

            // if it's a bot admin
            if (Config.IsAdmin(roomObject.Name))
            {
				// need to read amount
				if (Words.Length < 2)
					return;

				// try to get shills amount he want
				uint wantCash = 0;
				if (!UInt32.TryParse(Words[1], out wantCash))
					return;

				if (roomObject == null)
					return;
				
				// save this as our tradepartner (required for we offer first)
				Data.Trade.TradePartner = roomObject;
				
                // get our shillings from inventory
                InventoryObject shillings = Data.InventoryObjects.GetItemByName(NAME_SHILLING, false);

                // get how much cash we have
                uint haveCash = 0;
                if (shillings != null)
                    haveCash = shillings.Count;

                // tell we don't have that many
                uint offerCash = wantCash;                
                if (haveCash < wantCash)
                {
                    offerCash = haveCash;

                    // tell admin
                    SendSayGroupMessage(
                        AdminID, Config.ChatPrefixString + "Master " + roomObject.Name + ", I don't have that many shillings. I give you all I have.");
                }

                // the shillings we offer him
                ObjectID[] offer = new ObjectID[] { new ObjectID(shillings.ID, offerCash) };

                // send offer            
                SendReqOffer(new ObjectID(AdminID), offer);

                isGiveCashOffer = true;
            }
        }
		
        /// <summary>
        /// Returns an advertisement string for broadcast and whispers.
        /// </summary>
        /// <returns>Advertise message or NULL if no items</returns>
        protected string GetAdvertisement()
        {
            string head =
                Config.Shopname + " ~n~B~k- (~n " + Data.RoomInformation.RoomName + " ~B~k) ~n~B~k- (~n~I " +
                                "Just tell the bot ~b'buff'~w command and ~boffer " + Config.Buffprice + " shillings~w as he request and you will be buffed!~n ~B~k)~n";

            /// 
            /// Start building message
            /// 

            string message = head;

            // return message or null
            return message;
        }
		
        /// <summary>
        /// Returns an help string for chat to tell customer how to use bot.
        /// </summary>
        /// <returns></returns>
        protected string GetHelp()
        {
            string message = Config.Shopname + " ~n~B~k(~n" + Data.RoomInformation.RoomName + "~B~k)~n";

            message += Environment.NewLine + Environment.NewLine;

            message += "~B~rUnknown command!~n" + Environment.NewLine + Environment.NewLine;

            message += "~B~kWhisper (tell me) these commands:~n" + Environment.NewLine;
            message += "    ~n~B ~r• ~k" + COMMAND_BUFF + " ~b— Buy the buffs.";
            message += Environment.NewLine + Environment.NewLine;
            message += "~n~B~kTo donate your money, reagents or Inkies just offer them to me!";

            return message;
        }
		
        /// <summary>
        /// Whispers advertisement to a player
        /// </summary>
        /// <param name="RoomObject"></param>
        protected void TellAdvertisement(RoomObject RoomObject)
        {
            string message = GetAdvertisement();

            if (message != null)
            {
                // tell player
                SendSayGroupMessage(RoomObject.ID, message);

                // log
                Log("BOT", "I whispered advertise to player " + RoomObject.Name);
            }
        }

        /// <summary>
        /// Overrides Update from BaseClient to trigger advertising in intervals.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // make sure to not execute it too early before fully logged in
            if (ObjectID.IsValid(Data.AvatarID) &&
                Data.InventoryObjects.Count > 0)
            {
                string message;
                
                // time to broadcast
                if (GameTick.CanBroadcast())
                {
                    // get adv. message
                    message = GetAdvertisement();

                    // content?
                    if (message != null)
                    {
                        // try to broadcast buffbot advert
                        SendSayToMessage(ChatTransmissionType.Everyone, message);

                        // log
                        Log("BOT", LOG_ADVBROADCASTED);

                        // additionally mark said also
                        GameTick.DidSay(); 
                    }
                }

                if (!isMeditateInProcess) {
                    if (Data.VigorPoints < 60)
                    {
                        InventoryObject inventoryObject = Data.InventoryObjects.GetItemByName("inky-cap mushroom", false);

                        Thread InkyThread = new Thread(delegate () { UseInky(inventoryObject); });
                        InkyThread.Start();
                    }
                }

                if (!isMeditateInProcess && !isBuffingInProcess) {
                    if (Data.ManaPoints < 50) {
                        Thread MeditateThread = new Thread(new ThreadStart(CastMeditate));
                        MeditateThread.Start();
                    }
                }
            }
        }
    }
}

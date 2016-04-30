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
using System.Xml;
using Meridian59.Common;

namespace Meridian59.Bot.Buff
{
    /// <summary>
    /// Reads BuffBot configuration file
    /// </summary>
    public class BuffBotConfig : BotConfig
    {
        protected const string XMLATTRIB_INTERVALBROADCAST = "intervalbroadcast";
        protected const string XMLATTRIB_TELLONENTER = "tellonenter";
        protected const string XMLATTRIB_CHATPREFIXSTRING = "chatprefixstring";
        protected const string XMLATTRIB_SHOPNAME = "shopname";
        protected const string XMLATTRIB_ENABLEDONATIONS = "enabledonations";
        protected const string XMLATTRIB_BUFFPRICE = "buffprice";

        public const uint DEFAULTVAL_SHOPBOT_INTERVALBROADCAST = 3600;
        public const bool DEFAULTVAL_SHOPBOT_TELLONENTER = true;
        public const string DEFAULTVAL_SHOPBOT_CHATPREFIXSTRING = "~B~r";
        public const string DEFAULTVAL_SHOPBOT_SHOPNAME = "~B~rB~n~kuff~B~rB~n~kot";
        public const bool DEFAULTVAL_SHOPBOT_ENABLEDONATIONS = true;
        public const uint DEFAULTVAL_SHOPBOT_BUFFPRICE = 3500;
		
        public uint IntervalBroadcast { get; protected set; }
        public bool TellOnEnter { get; protected set; }
        public string ChatPrefixString { get; protected set; }
        public string Shopname { get; protected set; }
        public bool Enabledonations { get; protected set; }
        public uint Buffprice { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public BuffBotConfig()
            : base()
        { }

        /// <summary>
        /// 
        /// </summary>
        protected override void InitPreConfig()
        {
            base.InitPreConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void InitPastConfig()
        {
            base.InitPastConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Document"></param>
        public override void ReadXml(XmlDocument Document)
        {
            // read baseclass part
            base.ReadXml(Document);

            uint val_uint;
            bool val_bool;
            XmlNode node;

            // basics

            node = Document.DocumentElement.SelectSingleNode(
                '/' + XMLTAG_CONFIGURATION + '/' + XMLTAG_BOT);

            if (node != null)
            {
                IntervalBroadcast = (node.Attributes[XMLATTRIB_INTERVALBROADCAST] != null && UInt32.TryParse(node.Attributes[XMLATTRIB_INTERVALBROADCAST].Value, out val_uint)) ?
                    val_uint * GameTick.MSINSECOND : DEFAULTVAL_SHOPBOT_INTERVALBROADCAST * GameTick.MSINSECOND;

                TellOnEnter = (node.Attributes[XMLATTRIB_TELLONENTER] != null && Boolean.TryParse(node.Attributes[XMLATTRIB_TELLONENTER].Value, out val_bool)) ?
                    val_bool : DEFAULTVAL_SHOPBOT_TELLONENTER;

                ChatPrefixString = (node.Attributes[XMLATTRIB_CHATPREFIXSTRING] != null) ?
                    node.Attributes[XMLATTRIB_CHATPREFIXSTRING].Value : DEFAULTVAL_SHOPBOT_CHATPREFIXSTRING;

                Shopname = (node.Attributes[XMLATTRIB_SHOPNAME] != null) ?
                    node.Attributes[XMLATTRIB_SHOPNAME].Value : DEFAULTVAL_SHOPBOT_SHOPNAME;

                Enabledonations = (node.Attributes[XMLATTRIB_ENABLEDONATIONS] != null && Boolean.TryParse(node.Attributes[XMLATTRIB_ENABLEDONATIONS].Value, out val_bool)) ?
                    val_bool : DEFAULTVAL_SHOPBOT_ENABLEDONATIONS;

                Buffprice = (node.Attributes[XMLATTRIB_BUFFPRICE] != null && UInt32.TryParse(node.Attributes[XMLATTRIB_BUFFPRICE].Value, out val_uint)) ?
                    val_uint : DEFAULTVAL_SHOPBOT_BUFFPRICE;
            }
            else
            {
                IntervalBroadcast = DEFAULTVAL_SHOPBOT_INTERVALBROADCAST * GameTick.MSINSECOND;
                TellOnEnter = DEFAULTVAL_SHOPBOT_TELLONENTER;
                ChatPrefixString = DEFAULTVAL_SHOPBOT_CHATPREFIXSTRING;
                Shopname = DEFAULTVAL_SHOPBOT_SHOPNAME;
                Enabledonations = DEFAULTVAL_SHOPBOT_ENABLEDONATIONS;
                Buffprice = DEFAULTVAL_SHOPBOT_BUFFPRICE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Writer"></param>
        public override void WriteXml(XmlWriter Writer)
        {
            base.WriteXml(Writer);
        }
    }
}
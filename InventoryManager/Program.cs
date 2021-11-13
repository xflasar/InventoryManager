using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

// This script will:
// Get all items from ship
// it will show it onto LCD screen with multiple pages
// Move items from ship blocks with inventory to a Cargo containers
// Sort every item with possibility to have 1 or more cargo containers for item dedicated


namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        Dictionary<string, IMyCargoContainer> m_CargoContainers = new Dictionary<string, IMyCargoContainer>();
        Dictionary<string, IMyAssembler> m_Assemblers = new Dictionary<string, IMyAssembler>();
        Dictionary<string, IMyRefinery> m_Refineries = new Dictionary<string, IMyRefinery>();
        Dictionary<string, IMyGasGenerator> m_GasGenerators = new Dictionary<string, IMyGasGenerator>();
        Dictionary<string, IMyShipConnector> m_Connectors = new Dictionary<string, IMyShipConnector>();
        Dictionary<string, float> m_items = new Dictionary<string, float>(); // Items available in Cargo Containers
        Dictionary<string, float> m_itemsRequired = new Dictionary<string, float>(); // This will set up required items to have in Cargo Containers if actual item amount is lower than required one it will add que to assemblers to level off the difference to match required amount of item
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        public Program()
        {
            // Reruns the script every 10 ticks -> 167 ms
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            // Get all the blocks 
            // This can portrait a problem if we add more blocks so we need to add this to a function
            GetAllBlocks();
            SortBlocks();

        }
        void GetAllBlocks()
        {
           GridTerminalSystem.GetBlocks(blocks);
        }
        void SortBlocks()
        {
            blocks.ForEach((block) => { if (block is IMyCargoContainer && !m_CargoContainers.ContainsKey(block.Name) && block.OwnerId == this.Me.OwnerId) { m_CargoContainers.Add(block.Name, block as IMyCargoContainer); } });
            blocks.ForEach((block) => { if (block is IMyRefinery && block.OwnerId == this.Me.OwnerId && !m_Refineries.ContainsKey(block.CustomName)) m_Refineries.Add(block.CustomName, block as IMyRefinery); });
            blocks.ForEach((block) => { if (block is IMyGasGenerator && block.OwnerId == this.Me.OwnerId && !m_GasGenerators.ContainsKey(block.CustomName)) m_GasGenerators.Add(block.CustomName, block as IMyGasGenerator); });
            blocks.ForEach((block) => { if (block is IMyAssembler && block.OwnerId == this.Me.OwnerId && !m_Assemblers.ContainsKey(block.CustomName)) m_Assemblers.Add(block.CustomName, block as IMyAssembler); });
        }
        // Method for getting all the items in containers excluding connectors and assemblers due to auto-transfer
        void GetItemsFromShipCargoContainers()
        {
            // Iterate thru all containers and get total count of each items in inventory then save it into a Dictionary while Adding or decreasing the amount of items by each pass tho I would say it would be easier to just set new value by getting it again thru sum but can be holding each container as in array 
            foreach (KeyValuePair<string, IMyCargoContainer> keyValuePair in m_CargoContainers)
            {
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                keyValuePair.Value.GetInventory().GetItems(items);
                items.ForEach((item) => {
                    if (!m_items.ContainsKey(item.Type.ToString()))
                    {
                        m_items.Add(item.Type.ToString(), (float)item.Amount);
                    }
                    else if (m_items[item.Type.ToString()] > (float)item.Amount)
                    {
                        m_items[item.Type.ToString()] -= (m_items[item.Type.ToString()] - (float)item.Amount);
                    }
                    else if (m_items[item.Type.ToString()] < (float)item.Amount)
                    {
                        m_items[item.Type.ToString()] += ((float)item.Amount - m_items[item.Type.ToString()]);
                    }
                });
            }
        }
        public void Main(string argument, UpdateType updateSource)
        {
            GetItemsFromShipCargoContainers();
        }
    }
}

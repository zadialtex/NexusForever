using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Game.Spell.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Entity
{
    public class ClientInteractionEvent
    {
        public uint ClientUniqueId { get; private set; } = 0;
        public WorldEntity ActivateUnit { get; private set; }
        private Player Owner { get; set; } = null;
        private Spell.Spell QueuedSpell { get; set; } = null;
        private bool TriggerInstantly { get; set; } = false;

        public ClientInteractionEvent(Player owner, WorldEntity activateUnit, uint clientUniqueId, uint spell4Id, bool triggerInstantly = false)
        {
            Owner = owner;
            ActivateUnit = activateUnit;
            ClientUniqueId = clientUniqueId;
            TriggerInstantly = triggerInstantly;

            Spell4Entry spell4Entry = GameTableManager.Spell4.GetEntry(spell4Id);
            if (spell4Entry == null)
                throw new ArgumentOutOfRangeException();

            SpellBaseInfo spellBaseInfo = GlobalSpellManager.GetSpellBaseInfo(spell4Entry.Spell4BaseIdBaseSpell);
            if (spellBaseInfo == null)
                throw new ArgumentOutOfRangeException();

            SpellInfo spellInfo = spellBaseInfo.GetSpellInfo((byte)spell4Entry.TierIndex);
            if (spellInfo == null)
                throw new ArgumentOutOfRangeException();

            QueuedSpell = new Spell.Spell(Owner, new SpellParameters
            {
                SpellInfo = spellInfo,
                PrimaryTargetId = ActivateUnit.Guid,
                ClientUniqueId = ClientUniqueId,
            });

            Trigger();
        }

        public void Trigger()
        {
            QueuedSpell.Cast();

            if (TriggerInstantly)
                TriggerSuccess();
        }

        public void TriggerReady()
        {

        }

        public void TriggerFail()
        {
            ActivateUnit.OnActivateFail(Owner);
        }

        public void TriggerSuccess()
        {
            ActivateUnit.OnActivateSuccess(Owner);
            QueuedSpell.Execute();
        }
    }
}

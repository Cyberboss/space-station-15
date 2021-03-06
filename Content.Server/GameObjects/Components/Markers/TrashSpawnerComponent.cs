using System;
using Content.Server.GameObjects.Components.Markers;
using System.Collections.Generic;
using Content.Server.GameTicking;
using Content.Server.Interfaces.GameTicking;
using Robust.Server.Interfaces.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.Random;
using Robust.Shared.Maths;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.ViewVariables;
using Logger = Robust.Shared.Log.Logger;

namespace Content.Server.GameObjects.Components.Markers
{
    [RegisterComponent]
    public class TrashSpawnerComponent : ConditionalSpawnerComponent
    {
        public override string Name => "TrashSpawner";

#pragma warning disable 649
        [Dependency] private IEntityManager _entityManager;
        [Dependency] private IRobustRandom _robustRandom;
#pragma warning restore 649

        [ViewVariables(VVAccess.ReadWrite)]
        public List<string> RarePrototypes { get; set; } = new List<string>();

        [ViewVariables(VVAccess.ReadWrite)]
        private List<string> _gameRules = new List<string>();

        [ViewVariables(VVAccess.ReadWrite)]
        public float RareChance { get; set; } = 0.05f;

        [ViewVariables(VVAccess.ReadWrite)]
        public float Offset { get; set; } = 0.2f;

        public override void ExposeData(ObjectSerializer serializer)
        {
            base.ExposeData(serializer);
            serializer.DataField(this, x => RarePrototypes, "rarePrototypes", new List<string>());
            serializer.DataField(this, x => RareChance, "rareChance", 0.05f);
            serializer.DataField(this, x => Offset, "offset", 0.2f);
        }
        public override void Spawn()
        {
            if (RarePrototypes.Count > 0 && (RareChance == 1.0f || _robustRandom.Prob(RareChance)))
            {
                _entityManager.SpawnEntity(_robustRandom.Pick(RarePrototypes), Owner.Transform.GridPosition);
                return;
            }

            if (Chance != 1.0f && !_robustRandom.Prob(Chance))
            {
                return;
            }

            if (Prototypes.Count == 0)
            {
                Logger.Warning($"Prototype list in TrashSpawnComponent is empty! Entity: {Owner}");
                return;
            }

            if(!Owner.Deleted)
            {
                var random = IoCManager.Resolve<IRobustRandom>();

                var x_negative = random.Prob(0.5f) ? -1 : 1;
                var y_negative = random.Prob(0.5f) ? -1 : 1;

                var entity = _entityManager.SpawnEntity(_robustRandom.Pick(Prototypes), Owner.Transform.GridPosition);
                entity.Transform.LocalPosition += new Vector2(random.NextFloat() * Offset * x_negative, random.NextFloat() * Offset * y_negative);
            }

        }

        public override void MapInit()
        {
            Spawn();
            Owner.Delete();
        }
    }
}

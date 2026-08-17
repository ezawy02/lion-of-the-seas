using System;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;

namespace SeaLion.Core.Bootstrap
{
    public interface ISceneTransition { bool TryLoad(string sceneId); }
    public interface IQualitySelector { void Apply(string preference); }
    public sealed class BootstrapComposition
    {
        private readonly LocalSaveRepository save; private readonly ISceneTransition scenes; private readonly IQualitySelector quality;
        public PlayerSaveData Player { get; private set; }
        public BootstrapComposition(LocalSaveRepository save, ISceneTransition scenes, IQualitySelector quality)
        { this.save = save ?? throw new ArgumentNullException("save"); this.scenes = scenes ?? throw new ArgumentNullException("scenes"); this.quality = quality ?? throw new ArgumentNullException("quality"); }
        public bool Start(string frontendScene, string directLevelScene)
        { var loaded = save.Load(); Player = loaded.Data ?? LocalSaveRepository.CreateDefault(); quality.Apply(Player.settings.qualityPreference); return scenes.TryLoad(string.IsNullOrEmpty(directLevelScene) ? frontendScene : directLevelScene); }
        public bool Save() => Player != null && save.Save(Player, out _);
        public bool LaunchLevel(string sceneId) => !string.IsNullOrEmpty(sceneId) && scenes.TryLoad(sceneId);
        public bool TryCreateSession(string levelId, string phaseId, out BattleSession session)
        { session = null; if (!StableId.IsValid(levelId) || !StableId.IsValid(phaseId) || Player == null) return false; session = new BattleSession(new StableId(levelId), new StableId(phaseId), Player.selectedLoadout.ToSnapshot()); return session.TryTransition(BattleState.Ready); }
    }
}

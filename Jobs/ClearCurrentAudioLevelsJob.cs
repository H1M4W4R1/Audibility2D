using Systems.Audibility2D.Data.Native;
using Systems.Audibility2D.Utility;
using Unity.Collections;
using Unity.Jobs;

namespace Systems.Audibility2D.Jobs
{
    public struct ClearCurrentAudioLevelsJob : IJobParallelFor
    {
        public NativeArray<AudioTileInfo> tileComputeData;
        
        public void Execute(int index)
        {
            AudioTileInfo tileInfo = tileComputeData[index];
            tileInfo.currentAudioLevel = AudibilityTools.LOUDNESS_NONE;
            tileComputeData[index] = tileInfo;
        }
    }
}
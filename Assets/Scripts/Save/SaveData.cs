using System.Collections.Generic;

/// <summary>
/// Plain-data container serialised to JSON via JsonUtility.
/// All fields must be public for JsonUtility reflection.
/// </summary>
[System.Serializable]
public class SaveData
{
    public int    prestigeLevel;
    public int    permanentGenes;
    public long   lastSaveTimestamp;   // Unix epoch seconds (UTC)

    public List<SerializedDNA>  dnaStorage  = new();
    public List<Creature>       creatures   = new();
    public List<SerializedTool> toolStates  = new();

    [System.Serializable]
    public class SerializedDNA
    {
        public DNAType type;
        public float   amount;
    }

    [System.Serializable]
    public class SerializedTool
    {
        public ToolType toolType;
        public bool     isUnlocked;
        public bool     isActive;
    }
}

using System;
using UnityEngine.Serialization;

namespace ZGraph
{
    [Serializable]
    public struct EdgeData : IEquatable<EdgeData>
    {
        [FormerlySerializedAs("outputPortViewDataKey")] public string outputPortID;
        [FormerlySerializedAs("inputPortViewDataKey")] public string inputPortID;

        public EdgeData(string inputPortID, string outputPortID)
        {
            this.inputPortID = inputPortID;
            this.outputPortID = outputPortID;
        }

        public bool Equals(EdgeData other)
        {
            return outputPortID == other.outputPortID && inputPortID == other.inputPortID;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(outputPortID, inputPortID);
        }
    }
}
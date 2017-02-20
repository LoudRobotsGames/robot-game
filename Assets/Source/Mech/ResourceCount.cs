using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class ResourceCount
{
    public ResourceType Type;
    public int Count;

    public ResourceCount()
    {
        Type = ResourceType.Wiring;
        Count = 0;
    }

    public ResourceCount(ResourceType type, int count)
    {
        Type = type;
        Count = count;
    }

    public ResourceCount(ResourceCount resource)
    {
        Type = resource.Type;
        Count = resource.Count;
    }
}

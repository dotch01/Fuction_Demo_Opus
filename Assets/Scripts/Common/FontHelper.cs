using UnityEngine;

public static class FontHelper
{
    private static Font _cached;

    public static Font GetFont()
    {
        if (_cached != null) return _cached;

        _cached = Resources.Load<Font>("Fonts/NotoSansTC-Regular");
        if (_cached == null)
            _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_cached == null)
            _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return _cached;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ZaoHuaMod.GuiFramework.Controls
{
    public static class UITooltip
    {
        private struct Item
        {
            public Rect Rect;
            public string Text;
        }


        private static List<Item> _items = new List<Item>();


        public static void Register(Rect rect,string text)
        {
            _items.Add(new Item
            {
                Rect=rect,
                Text=text
            });
        }


        public static void Begin()
        {
            _items.Clear();
        }


        public static void Draw()
        {
            if(Event.current.type != EventType.Repaint)
                return;

            Vector2 mouse = Event.current.mousePosition;

            foreach(var item in _items)
            {
                if(item.Rect.Contains(mouse))
                {
                    DrawTooltip(item.Text);
                    return;
                }
            }
        }


        private static void DrawTooltip(string text)
        {
            // 在 GUI.Window 内直接以本地坐标绘制，避免屏幕坐标换算出错
            Vector2 pos = Event.current.mousePosition;
            float scale = UI.WindowControls.Scale;
            Vector2 size = GUI.skin.box.CalcSize(new GUIContent(text));

            GUI.Box(
                new Rect(
                    pos.x + 15f / scale,
                    pos.y + 15f / scale,
                    size.x + 20,
                    size.y + 10
                ),
                text
            );
        }
    }
}
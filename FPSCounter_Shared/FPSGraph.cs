using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPSCounter
{
    internal static class FPSGraph
    {
        private static readonly Queue<float> _fpsHistory = new Queue<float>();
        private static int _maxHistorySize = 120;
        private static Rect _graphRect;
        private static readonly Color _graphLineColor = new Color(0.2f, 0.9f, 0.2f, 1f);
        private static readonly Color _graphFillColor = new Color(0.2f, 0.9f, 0.2f, 0.12f);
        private static readonly Color _graphBackground = new Color(0.05f, 0.05f, 0.05f, 0.6f);
        private static readonly Color _gridColor = new Color(1f, 1f, 1f, 0.1f);
        private static readonly Color _borderColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
        private static Texture2D _pixelTexture;
        private static Texture2D _lineTexture;
        private static Texture2D _fillTexture;

        private static float _currentMinFPS = 0f;
        private static float _currentMaxFPS = 60f;
        private static float _targetMinFPS = 0f;
        private static float _targetMaxFPS = 60f;

        public static bool ShowGraph { get; set; }
        public static int MaxHistorySize
        {
            get => _maxHistorySize;
            set
            {
                _maxHistorySize = Mathf.Clamp(value, 30, 600);
                while (_fpsHistory.Count > _maxHistorySize)
                    _fpsHistory.Dequeue();
            }
        }

        public static void Initialize()
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(1, 1);
                _pixelTexture.SetPixel(0, 0, Color.white);
                _pixelTexture.Apply();
            }
            if (_lineTexture == null)
            {
                _lineTexture = new Texture2D(1, 1);
                _lineTexture.SetPixel(0, 0, Color.white);
                _lineTexture.Apply();
            }
            if (_fillTexture == null)
            {
                _fillTexture = new Texture2D(1, 1);
                _fillTexture.SetPixel(0, 0, Color.white);
                _fillTexture.Apply();
            }
        }

        public static void AddFPS(float fps)
        {
            if (!ShowGraph) return;

            _fpsHistory.Enqueue(fps);
            while (_fpsHistory.Count > _maxHistorySize)
                _fpsHistory.Dequeue();
        }

        public static void UpdatePosition(TextAnchor position, int screenOffset)
        {
            int w = Screen.width;
            int h = Screen.height;
            int graphWidth = Mathf.Min(_maxHistorySize * 3, w / 2);
            int graphHeight = Mathf.Max(20, h / 25);

            switch (position)
            {
                case TextAnchor.UpperLeft:
                    _graphRect = new Rect(screenOffset, screenOffset + h / 18, graphWidth, graphHeight);
                    break;
                case TextAnchor.UpperCenter:
                    _graphRect = new Rect((w - graphWidth) / 2, screenOffset + h / 18, graphWidth, graphHeight);
                    break;
                case TextAnchor.UpperRight:
                    _graphRect = new Rect(w - graphWidth - screenOffset, screenOffset + h / 18, graphWidth, graphHeight);
                    break;
                case TextAnchor.MiddleLeft:
                    _graphRect = new Rect(screenOffset, (h - graphHeight) / 2, graphWidth, graphHeight);
                    break;
                case TextAnchor.MiddleCenter:
                    _graphRect = new Rect((w - graphWidth) / 2, (h - graphHeight) / 2, graphWidth, graphHeight);
                    break;
                case TextAnchor.MiddleRight:
                    _graphRect = new Rect(w - graphWidth - screenOffset, (h - graphHeight) / 2, graphWidth, graphHeight);
                    break;
                case TextAnchor.LowerLeft:
                    _graphRect = new Rect(screenOffset, h - graphHeight - screenOffset - h / 40, graphWidth, graphHeight);
                    break;
                case TextAnchor.LowerCenter:
                    _graphRect = new Rect((w - graphWidth) / 2, h - graphHeight - screenOffset - h / 40, graphWidth, graphHeight);
                    break;
                case TextAnchor.LowerRight:
                    _graphRect = new Rect(w - graphWidth - screenOffset, h - graphHeight - screenOffset - h / 40, graphWidth, graphHeight);
                    break;
                default:
                    _graphRect = new Rect(w - graphWidth - screenOffset, h - graphHeight - screenOffset, graphWidth, graphHeight);
                    break;
            }
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.001f) return;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            Matrix4x4 matrix = GUI.matrix;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y - thickness * 0.5f, length, thickness), _lineTexture, ScaleMode.StretchToFill);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }

        private static void DrawFilledArea(Vector2[] points, float bottomY, Color color)
        {
            if (points.Length < 2) return;

            GUI.color = color;
            for (int i = 0; i < points.Length - 1; i++)
            {
                float x1 = points[i].x;
                float x2 = points[i + 1].x;
                float y1 = points[i].y;
                float y2 = points[i + 1].y;

                float minX = Mathf.Min(x1, x2);
                float maxX = Mathf.Max(x1, x2);
                float minY = Mathf.Min(y1, y2);

                float segmentWidth = maxX - minX + 1;
                float segmentHeight = bottomY - minY;

                if (segmentWidth > 0 && segmentHeight > 0)
                {
                    GUI.DrawTexture(new Rect(minX, minY, segmentWidth, segmentHeight), _fillTexture, ScaleMode.StretchToFill);
                }
            }
            GUI.color = Color.white;
        }

        public static void Draw()
        {
            if (!ShowGraph || _fpsHistory.Count < 2) return;

            Initialize();

            float[] fpsArray = _fpsHistory.ToArray();

            float maxFPS = 0f;
            float minFPS = float.MaxValue;
            float avgFPS = 0f;

            for (int i = 0; i < fpsArray.Length; i++)
            {
                float fps = fpsArray[i];
                if (fps > maxFPS) maxFPS = fps;
                if (fps < minFPS) minFPS = fps;
                avgFPS += fps;
            }
            avgFPS /= fpsArray.Length;

            if (maxFPS < 30f) maxFPS = 30f;
            if (minFPS > 0f) minFPS = 0f;

            _targetMinFPS = minFPS;
            _targetMaxFPS = maxFPS * 1.1f;

            _currentMinFPS = Mathf.Lerp(_currentMinFPS, _targetMinFPS, 0.05f);
            _currentMaxFPS = Mathf.Lerp(_currentMaxFPS, _targetMaxFPS, 0.05f);

            if (Mathf.Abs(_currentMaxFPS - _targetMaxFPS) < 1f) _currentMaxFPS = _targetMaxFPS;
            if (Mathf.Abs(_currentMinFPS - _targetMinFPS) < 1f) _currentMinFPS = _targetMinFPS;

            float fpsRange = _currentMaxFPS - _currentMinFPS;
            if (fpsRange < 10f) fpsRange = 10f;

            float xStep = _graphRect.width / (_maxHistorySize - 1);
            float yScale = _graphRect.height / fpsRange;

            Vector2[] points = new Vector2[fpsArray.Length];
            for (int i = 0; i < fpsArray.Length; i++)
            {
                float x = _graphRect.x + i * xStep;
                float y = _graphRect.y + _graphRect.height - (fpsArray[i] - _currentMinFPS) * yScale;
                points[i] = new Vector2(x, y);
            }

            float bottomY = _graphRect.y + _graphRect.height;

            GUI.color = _graphBackground;
            GUI.DrawTexture(_graphRect, _pixelTexture, ScaleMode.StretchToFill);
            GUI.color = Color.white;

            DrawFilledArea(points, bottomY, _graphFillColor);

            float lineThickness = Mathf.Max(1f, _graphRect.height / 40f);

            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(points[i], points[i + 1], _graphLineColor, lineThickness);
            }

            DrawLine(new Vector2(_graphRect.x, _graphRect.y), new Vector2(_graphRect.x + _graphRect.width, _graphRect.y), _borderColor, 1f);
            DrawLine(new Vector2(_graphRect.x, _graphRect.y + _graphRect.height), new Vector2(_graphRect.x + _graphRect.width, _graphRect.y + _graphRect.height), _borderColor, 1f);
            DrawLine(new Vector2(_graphRect.x, _graphRect.y), new Vector2(_graphRect.x, _graphRect.y + _graphRect.height), _borderColor, 1f);
            DrawLine(new Vector2(_graphRect.x + _graphRect.width, _graphRect.y), new Vector2(_graphRect.x + _graphRect.width, _graphRect.y + _graphRect.height), _borderColor, 1f);

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(_graphRect.height * 0.35f));
            labelStyle.normal.textColor = new Color(0.2f, 0.9f, 0.2f, 1f);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.LowerLeft;

            string avgText = $"Avg: {avgFPS:F1}";
            Vector2 textSize = labelStyle.CalcSize(new GUIContent(avgText));
            float labelX = _graphRect.x + 2;
            float labelY = _graphRect.y + _graphRect.height - textSize.y - 1;

            GUI.Label(new Rect(labelX, labelY, textSize.x, textSize.y), avgText, labelStyle);
        }

        public static void Clear()
        {
            _fpsHistory.Clear();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace FPS.UI
{
    public sealed class FillBarColorChange : MonoBehaviour
    {
        [Header("Foreground")]
        [Tooltip("前景图片")]
        public Image foregroundImage;

        [Tooltip("默认前景颜色")]
        public Color defaultForegroundColor;

        [Tooltip("满时前景颜色")]
        public Color flashForegroundColorFull;

        [Header("Background")]
        [Tooltip("背景图")]
        public Image backgroundImage;

        [Tooltip("默认背景颜色")]
        public Color defaultBackgroundColor;

        [Tooltip("空时背景颜色")]
        public Color flashBackgroundColorEmpty;

        [Header("Values")]
        [Tooltip("满时阈值")]
        public float fullValue = 1f;

        [Tooltip("空时阈值")]
        public float emptyValue = 0f;

        [Tooltip("颜色改变灵敏度")]
        public float colorChangeSharpness = 5f;

        float m_PreviousValue;

        public void Initialize(float fullValueRatio, float emptyValueRatio)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRatio;

            m_PreviousValue = fullValueRatio;
        }

        public void UpdateVisual(float currentRatio)
        {
            if (Mathf.Approximately(currentRatio, fullValue) && !Mathf.Approximately(currentRatio, m_PreviousValue))
            {
                foregroundImage.color = flashForegroundColorFull;
            }
            else if (currentRatio < emptyValue)
            {
                backgroundImage.color = flashBackgroundColorEmpty;
            }
            else
            {
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForegroundColor,
                    Time.deltaTime * colorChangeSharpness);
                backgroundImage.color = Color.Lerp(backgroundImage.color, defaultBackgroundColor,
                    Time.deltaTime * colorChangeSharpness);
            }

            m_PreviousValue = currentRatio;
        }
    }
}
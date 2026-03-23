using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Unity'nin varsayılan GridLayoutGroup'unun aksine, tam dolmayan son satırdaki elemanları
    /// sola yanaştırmak yerine ortalayarak dalkavukluk yapmayan (CSS Flex Wrap vb.) özel bir Layout Group.
    /// CSS'teki "justify-content: center; flex-wrap: wrap;" mantığıyla çalışır.
    /// </summary>
    public class FlexGridLayout : LayoutGroup
    {
        [Header("Ayarlar")]
        public Vector2 cellSize = new Vector2(220, 80);
        public Vector2 spacing = new Vector2(20, 20);
        public int maxColumns = 4; // Bir satırda en fazla kaç hedef olacak? (örn: 4)

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputVertical() { }

        public override void SetLayoutHorizontal()
        {
            SetLayout(0);
        }

        public override void SetLayoutVertical()
        {
            SetLayout(1);
        }

        private void SetLayout(int axis)
        {
            int childCount = rectChildren.Count;
            if (childCount == 0) return;

            float containerWidth = rectTransform.rect.width;

            int rows = Mathf.CeilToInt((float)childCount / maxColumns);

            int childIndex = 0;
            // Tüm satırları dolaş
            for (int y = 0; y < rows; y++)
            {
                // Bu satırdaki eleman sayısı (son satırda maxColumns'tan daha az olabilir)
                int itemsInThisRow = Mathf.Min(maxColumns, childCount - childIndex);

                // Bu satırın toplam genişliği
                float rowWidth = (itemsInThisRow * cellSize.x) + ((itemsInThisRow - 1) * spacing.x);
                
                // Başlangıç noktasını ortala
                float startX = (containerWidth - rowWidth) / 2f + padding.left;
                float startY = padding.top + y * (cellSize.y + spacing.y);

                for (int x = 0; x < itemsInThisRow; x++)
                {
                    RectTransform child = rectChildren[childIndex];
                    
                    if (axis == 0)
                        SetChildAlongAxis(child, 0, startX + x * (cellSize.x + spacing.x), cellSize.x);
                    else
                        SetChildAlongAxis(child, 1, startY, cellSize.y);
                    
                    childIndex++;
                }
            }
        }
    }
}

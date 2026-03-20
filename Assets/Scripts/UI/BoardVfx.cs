using System.Collections.Generic;
using UnityEngine;

namespace Game.Board
{
    /// <summary>
    /// BoardView'dan tamamen bağımsız, sadece parçacık (VFX) mantığını yöneten sınıf.
    /// </summary>
    public sealed class BoardVfx
    {
        private readonly BoardView _view;
        private static Material _roundParticleMat;

        public BoardVfx(BoardView view)
        {
            _view = view;
        }

        public void PlayMatchVfx(List<int> matches, int matchLen, bool isQuad, bool isComplex)
        {
            int count;
            float speed;

            // Efekt gücünü ve miktarını türüne göre ayarla
            if (isComplex)       { count = 18; speed = 400f; } // — T/L
            else if (isQuad)     { count = 14; speed = 350f; } // — Quad
            else if (matchLen>=5){ count = 25; speed = 500f; } //  — 5'li
            else if (matchLen==4){ count = 12; speed = 350f; } //  — 4'lü
            else                 { count = 8;  speed = 250f; } //  — 3'lü

            // Eşleşen taşlardan birincisinin rengini ve Tipini bul
            TileType tType = TileType.RootRed;
            if (matches.Count > 0)
            {
                MatchKey.Decode(matches[0], out int fx, out int fy);
                var firstUi = _view.GetTileUI(fx, fy);
                if (firstUi != null) tType = firstUi.Type;
            }

            // Tile türüne göre Ana Renk (Solid) ve Soluk/Pastel Renk (Pale) belirleme
            Color solidColor, paleColor;
            GetColorsForTile(tType, out solidColor, out paleColor);

            foreach (var p in matches)
            {
                // Artık _view referansıyla koordinat alıyoruz
                MatchKey.Decode(p, out int x, out int y);
                var localPos = _view.CellToLocal(x, y);
                SpawnBurst(localPos, solidColor, paleColor, count, speed);
            }
        }

        private void GetColorsForTile(TileType type, out Color solid, out Color pale)
        {
            switch (type)
            {
                case TileType.RootRed:    solid = new Color(1f, 0.2f, 0.2f);   pale = new Color(1f, 0.6f, 0.6f); break;
                case TileType.RootBlue:   solid = new Color(0.2f, 0.5f, 1f);   pale = new Color(0.6f, 0.8f, 1f); break;
                case TileType.RootGreen:  solid = new Color(0.2f, 0.8f, 0.2f); pale = new Color(0.6f, 1f, 0.6f);   break;
                case TileType.RootYellow: solid = new Color(1f, 0.9f, 0.1f);   pale = new Color(1f, 1f, 0.6f);   break;
                case TileType.RootPurple: solid = new Color(0.7f, 0.2f, 1f);   pale = new Color(0.8f, 0.5f, 1f); break;
                case TileType.RootOrange: solid = new Color(1f, 0.5f, 0.1f);   pale = new Color(1f, 0.8f, 0.5f); break;
                default:                  solid = Color.white;                 pale = Color.white;               break;
            }
        }

        private Material GetRoundParticleMaterial()
        {
            if (_roundParticleMat != null) return _roundParticleMat;

            Shader shader = Shader.Find("UI/Default") ?? Shader.Find("Sprites/Default");
            _roundParticleMat = new Material(shader);

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = (size / 2f) - 2f; 

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = alpha * alpha * (3f - 2f * alpha); 
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _roundParticleMat.mainTexture = tex;

            return _roundParticleMat;
        }

        private void SpawnBurst(Vector2 localPos, Color solidColor, Color paleColor, int particleCount, float speed)
        {
            var boardRoot = _view.BoardRoot;
            var canvas = _view.ParentCanvas;
            var canvasCam = _view.CanvasCam;
            float cellSize = _view.CellSize;

            Vector3 worldPos = boardRoot.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
            float s = boardRoot.lossyScale.x;

            var go = new GameObject("VFX_Burst");
            go.layer = canvas.gameObject.layer;
            Vector3 camForward = canvasCam != null ? canvasCam.transform.forward : Vector3.forward;
            go.transform.position = worldPos - camForward * 0.5f;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Sistem oynamadan ayar yapmak için durdur

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.material = GetRoundParticleMaterial();
            rend.sortingLayerID = canvas.sortingLayerID;
            rend.sortingOrder = canvas.sortingOrder + 100;

            var main = ps.main;
            main.duration = 0.8f; 
            main.loop = false;
            main.playOnAwake = false; // Ayarlardan önce oynamasın
            
           
            // 1. Ömür aralığını genişlet (Bazıları erken, bazıları geç sönsün)
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
            
            // 2. Hızları birbirinden farklılaştır (Kimi uzağa, kimi yakına gitsin)
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * s * 0.3f, speed * s * 1.2f);
            
            // 3. Boyutları belirgin yap
            main.startSize = new ParticleSystem.MinMaxCurve(cellSize * 0.3f * s, cellSize * 0.65f * s);
            
            main.startColor = new ParticleSystem.MinMaxGradient(solidColor, paleColor);
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission;
            em.rateOverTime = 0;
            em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = cellSize * 0.1f * s; 

            // 4. HAVA SÜRTÜNMESİ (DAMPEN): Parçacıklar fırladıktan sonra yavaşlayarak durur.
            var limitVel = ps.limitVelocityOverLifetime;
            limitVel.enabled = true;
            limitVel.limit = 0.1f;    
            limitVel.dampen = 0.25f;  

            // 5. BOYUT KÜÇÜLME: Havada asılı kalırken yavaşça küçülsünler
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.6f, 0.8f), new Keyframe(1, 0)));

            // 6. YAVAŞÇA SÖNME (Alpha Fade)
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f) },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.6f), 
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            col.color = grad;

            ps.Play();
        }
    }
}

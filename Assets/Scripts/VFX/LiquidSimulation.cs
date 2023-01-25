using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

using Helpers;
using Player;
using World;
using UnityEngine.Rendering.Universal;

namespace VFX
{
    public class LiquidSimulation : MonoBehaviour, IFilterLoggerTarget
    {
        [Header("Material Prototypes")]
        [SerializeField] private Material SimMat;
        private Material m_SimMat;
        [SerializeField] private Material[] SimListenerMats;
        private Material[] m_SimListenerMats;
        [SerializeField, Range(0f, 1f)] private float impulseStrength = 0f;

        [SerializeField] ScriptableRendererData scriptableRenderer;

        [Header("Debug")]
        [SerializeField] RawImage _testImage;

        private CustomRenderTexture _SimTex;
        public CustomRenderTexture SimTex => _SimTex;

        private Room CurrRoom => PlayerCore.SpawnManager.CurrentRoom;

        private void Awake()
        {
            m_SimMat = InstantiateMaterial(SimMat);
            m_SimListenerMats = new Material[SimListenerMats.Length];

            if (scriptableRenderer == null) throw new System.Exception("Requires reference to the 2D renderer data");

            for (int i = 0; i < SimListenerMats.Length; i++)
            {
                m_SimListenerMats[i] = InstantiateMaterial(SimListenerMats[i]);
                // replace render feature at run time
                var rfs = scriptableRenderer.rendererFeatures;
                foreach (var rf in rfs)
                {
                    if (rf is DFRenderObject)
                    {
                        var roFeature = rf as DFRenderObject;
                        if (roFeature.OverrideMaterialPrototype == SimListenerMats[i])
                        {
                            Debug.Log($"Replacing {roFeature.name}'s material prototype {roFeature.OverrideMaterialPrototype.name}");
                            roFeature.OverrideMaterialInstance = m_SimListenerMats[i];
                        }
                    }
                }
            }
        }

        private void OnEnable()
        {
            Room.RoomTransitionEvent += OnRoomTransition;
        }

        private void OnDisable()
        {
            Room.RoomTransitionEvent -= OnRoomTransition;
        }

        private void Update()
        {
            if (_SimTex != null)
            {
                if (_testImage != null)
                {
                    _testImage.texture = _SimTex;
                }

                float velMag = PlayerCore.Actor.velocity.magnitude;
                if (CurrRoom != null && velMag > 1f)
                {
                    //Note: y is flipped because it is the upper left corner.
                    float impulseU = (PlayerCore.Actor.transform.position.x - CurrRoom.transform.position.x) / _SimTex.width;
                    float impulseV = (CurrRoom.transform.position.y - PlayerCore.Actor.transform.position.y) / _SimTex.height;
                    FilterLogger.Log(this, $"Impulse Strength: {impulseStrength * velMag / 256}");
                    m_SimMat.SetVector("_Impulse", new Vector3(impulseU, impulseV, impulseStrength * velMag / 256));
                }
                else
                {
                    m_SimMat.SetVector("_Impulse", new Vector3(0, 0, 0));
                }

            }
        }

        private void OnRoomTransition(Room roomEntering)
        {
            Bounds bounds = roomEntering.GetComponent<Collider2D>().bounds;

            _SimTex = CreateLavaSimTexture((int)bounds.extents.x * 2, (int)bounds.extents.y * 2);

            foreach(var m in m_SimListenerMats)
            {
                m.SetVector("_RoomPos", roomEntering.transform.position);
                m.SetVector("_RoomSize", new Vector2(_SimTex.width, _SimTex.height));
                m.SetTexture("_SimulationTex", _SimTex);
            }
        }

        private CustomRenderTexture CreateLavaSimTexture(int width, int height)
        {
            CustomRenderTexture tex = new CustomRenderTexture(width, height);
            tex.material = m_SimMat;

            tex.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
            tex.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
            tex.initializationColor = Color.black;

            tex.updateMode = CustomRenderTextureUpdateMode.Realtime;
            tex.doubleBuffered = true;

            return tex;
        }

        public LogLevel GetLogLevel()
        {
            return LogLevel.Warning;
        }

        private Material InstantiateMaterial(Material m)
        {
            return new Material(m)
            {
                name = m.name + " (Instantiated)"
            };
        }
    }
}
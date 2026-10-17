// Offline arena prototype. No third-party assets, no network or account system.
using System;
using System.Collections.Generic;
using DragonMoba.Domain;
using UnityEngine;

namespace DragonMoba.Runtime
{
    public sealed class PrototypeGame : MonoBehaviour
    {
        private readonly Dictionary<int, GameObject> views = new Dictionary<int, GameObject>();
        private readonly Dictionary<int, string> lastForm = new Dictionary<int, string>();
        private readonly List<GameObject> decorations = new List<GameObject>();
        private GameWorld world;
        private ArtPresentation art;
        private OrganicArena organic;
        private bool visualMovePending;
        private V2 visualMoveStart;
        private Camera gameCamera;
        private bool shopOpen;
        private Vector2 shopScroll;
        private string selectedChampionId = "GKU_KID";
        private string shopCategory = "Todos";
        private readonly Dictionary<string, Texture2D> itemIcons = new Dictionary<string, Texture2D>();
        private bool showHelp;
        private bool environmentBuilt;
        private Vector3 mouseTarget;
        private string lastMessage = "Selecione um personagem para iniciar.";
        private float messageUntil;
        private GUIStyle titleStyle;
        private GUIStyle normalStyle;
        private GUIStyle smallStyle;

        private void Awake()
        {
            art = GetComponent<ArtPresentation>();
            organic = GetComponent<OrganicArena>();
            // The authoritative local simulation is exactly 30 ticks per second.
            Time.fixedDeltaTime = GameWorld.FixedStep;
            Application.targetFrameRate = 60;
            CreateCamera(); // A selection menu must render before a match starts.
        }

        private void Start()
        {
            // Keep the previous automated native fixture operational, but never skip
            // champion selection for a regular player even if the scene flag is set.
            var nativeTest = Array.IndexOf(Environment.GetCommandLineArgs(), "-dbm001-native-test") >= 0;
            if (organic != null && organic.startDemo && nativeTest)
                StartMatch("GKU_SUPER");
        }

        private void StartMatch(string championId)
        {
            world = new GameWorld(championId);
            art?.Confirm();
            BuildEnvironment();
            CreateCamera();
            RefreshViews();
            lastMessage = "Partida iniciada. Clique direito para mover; C carrega Ki.";
            messageUntil = Time.time + 6f;
        }

        private void FixedUpdate()
        {
            if (world == null || world.Winner.HasValue) return;
            world.Tick(Time.fixedDeltaTime);
            if (visualMovePending && V2.Distance(visualMoveStart, world.Player.Position) > .5f)
            {
                TraceVisual("MOVEMENT CONFIRMED | x=" + world.Player.Position.X + " z=" + world.Player.Position.Y);
                visualMovePending = false;
            }
            RefreshViews();
            var emitted = world.DrainEvents();
            foreach (var e in emitted) RenderEvent(e);
        }

        private void Update()
        {
            if (world == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { ReturnToSelection(); return; }
            if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;
            if (Input.GetKeyDown(KeyCode.P)) shopOpen = !shopOpen;
            if (Input.GetKeyDown(KeyCode.S)) world.Submit(new GameCommand {
                ActorId = world.PlayerId, Kind = CommandKind.Stop });
            if (world.Winner.HasValue)
            {
                if (Input.GetKeyDown(KeyCode.Return)) RestartMatch();
                return;
            }
            var hero = world.Player;
            if (hero == null) return;
            UpdateCamera(hero);
            if (Input.GetKeyDown(KeyCode.C)) SendCharge(true);
            if (Input.GetKeyUp(KeyCode.C)) SendCharge(false);
            var point = MousePoint();
            if (Input.GetMouseButtonDown(1) && !PointerOverGui())
            {
                // Multiple colliders can be hit: choose the nearest valid enemy rather
                // than treating the ground or a decorative collider as the target.
                var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
                var hits = Physics.RaycastAll(ray, 500f);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                EntityState enemy = null;
                foreach (var hit in hits)
                {
                    var handle = hit.collider.GetComponentInParent<EntityHandle>();
                    if (handle == null) continue;
                    var candidate = world.Find(handle.EntityId);
                    if (candidate == null || !candidate.Alive || candidate.Team == hero.Team) continue;
                    enemy = candidate;
                    break;
                }
                if (enemy != null)
                {
                    var accepted = world.Submit(new GameCommand { ActorId = hero.Id,
                        Kind = CommandKind.Attack, TargetId = enemy.Id });
                    TraceVisual("RIGHT CLICK ATTACK | accepted=" + accepted + " enemy=" + enemy.Id);
                }
                else
                {
                    var accepted = world.Submit(new GameCommand { ActorId = hero.Id, Kind = CommandKind.Move, Position = new V2(point.x, point.z) });
                    if (organic != null && accepted) { visualMoveStart = hero.Position; visualMovePending = true; }
                    TraceVisual("RIGHT CLICK | accepted=" + accepted + " target=" + point);
                }
            }
            var upgrade = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (Input.GetKeyDown(KeyCode.Q)) { if (upgrade) Upgrade(AbilitySlot.Q); else Cast(AbilitySlot.Q, point); }
            if (Input.GetKeyDown(KeyCode.W)) { if (upgrade) Upgrade(AbilitySlot.W); else Cast(AbilitySlot.W, point); }
            if (Input.GetKeyDown(KeyCode.E)) { if (upgrade) Upgrade(AbilitySlot.E); else Cast(AbilitySlot.E, point); }
            if (Input.GetKeyDown(KeyCode.R)) { if (upgrade) Upgrade(AbilitySlot.R); else Cast(AbilitySlot.R, point); }
        }

        private void SendCharge(bool enabled)
        {
            if (world == null) return;
            var accepted = world.Submit(new GameCommand { ActorId = world.PlayerId, Kind = CommandKind.Charge, Pressed = enabled });
            TraceVisual("CHARGE | pressed=" + enabled + " accepted=" + accepted + " Ki=" + world.Player.Ki);
        }

        private void Upgrade(AbilitySlot slot)
        {
            if (world == null || world.Player == null) return;
            var accepted = world.Submit(new GameCommand { ActorId = world.PlayerId,
                Kind = CommandKind.UpgradeAbility, Slot = slot });
            TraceVisual("UPGRADE " + slot + " | accepted=" + accepted);
            lastMessage = accepted ? slot + " aprimorada para o nivel " + world.Player.SkillRanks[(int)slot]
                : "Sem pontos ou nivel minimo para aprimorar " + slot + ".";
            messageUntil = Time.time + 2.5f;
        }

        private void Cast(AbilitySlot slot, Vector3 point)
        {
            if (world == null) return;
            var accepted = world.Submit(new GameCommand { ActorId = world.PlayerId, Kind = CommandKind.Cast, Slot = slot,
                Position = new V2(point.x, point.z) });
            TraceVisual("CAST " + slot + " | accepted=" + accepted + " Ki=" + world.Player.Ki);
            if (!accepted)
            {
                lastMessage = world.Player.SkillRanks[(int)slot] == 0
                    ? "Tecnica bloqueada: evolua com Ctrl+" + slot + "."
                    : "Tecnica indisponivel: Ki, alcance, tempo de recarga ou estado.";
                messageUntil = Time.time + 2.5f;
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void TraceVisual(string message)
        {
            if (organic != null) Debug.Log("VISUAL INPUT | " + message);
        }

        private bool PointerOverGui()
        {
            var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (shopOpen && mouse.x > Screen.width - 340f && mouse.y > 105f) return true;
            if (mouse.x < 360f && mouse.y < 195f) return true;
            if (mouse.y > Screen.height - 160f &&
                mouse.x > Screen.width * .5f - 230f && mouse.x < Screen.width * .5f + 230f) return true;
            return false;
        }

        private Vector3 MousePoint()
        {
            if (gameCamera == null) return Vector3.zero;
            var ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                mouseTarget = ray.GetPoint(distance);
                return mouseTarget;
            }
            return mouseTarget;
        }

        private void RefreshViews()
        {
            var aliveIds = new HashSet<int>();
            foreach (var e in world.Entities)
            {
                if (!e.Alive && e.Kind != EntityKind.Hero)
                {
                    if (views.TryGetValue(e.Id, out var retired)) retired.SetActive(false);
                    continue;
                }
                aliveIds.Add(e.Id);
                GameObject go;
                string previous;
                var changedForm = e.Kind == EntityKind.Hero && lastForm.TryGetValue(e.Id, out previous) && previous != e.FormId;
                if (changedForm && views.TryGetValue(e.Id, out go))
                {
                    Destroy(go);
                    views.Remove(e.Id);
                }
                if (!views.TryGetValue(e.Id, out go))
                {
                    go = CreateEntityVisual(e);
                    views[e.Id] = go;
                }
                var presentation = go.GetComponent<CharacterPresentation>();
                go.SetActive(e.Alive || presentation != null);
                var placeholder = go.GetComponent<PlaceholderVisual>() != null;
                go.transform.position = placeholder ? Position(e.Position, e.Kind) : new Vector3(e.Position.X, 0, e.Position.Y);
                if (presentation != null) presentation.Sync(e);
                var threat = go.GetComponent<TowerRangeVisual>();
                if (threat != null && world.Player != null)
                    threat.Show(e.Team != world.Player.Team && world.Player.Alive &&
                        V2.Distance(e.Position, world.Player.Position) <= e.AttackRange + 3.5f);
                if (placeholder)
                {
                    go.transform.localScale = Scale(e);
                    var renderer = go.GetComponent<Renderer>();
                    if (renderer != null) renderer.material.color = ColorFor(e);
                }
                if (e.Kind == EntityKind.Hero &&
                    (!lastForm.TryGetValue(e.Id, out previous) || previous != e.FormId))
                {
                    lastForm[e.Id] = e.FormId;
                    if (previous != null && art == null) SpawnPulse(e.Position, ColorFor(e), 2.5f);
                }
            }
            var remove = new List<int>();
            foreach (var pair in views)
                if (!aliveIds.Contains(pair.Key) && world.Find(pair.Key) == null) remove.Add(pair.Key);
            foreach (var id in remove)
            {
                Destroy(views[id]);
                views.Remove(id);
                lastForm.Remove(id);
            }
        }

        private GameObject CreateEntityVisual(EntityState state)
        {
            GameObject visual = null;
            if (state.Kind == EntityKind.Hero)
            {
                // Optional licensed model: Assets/Resources/CharacterForms/<HeroId>/<FormId>.prefab
                // Every transformation selects its own prefab; empty path uses a safe placeholder.
                var asset = organic != null ? organic.Character(state) : null;
                if (asset == null) asset = Resources.Load<GameObject>("CharacterForms/" + state.HeroId + "/" + state.FormId);
                if (asset != null) visual = Instantiate(asset);
            }
            if (visual == null)
            {
                var primitive = state.Kind == EntityKind.Hero ? PrimitiveType.Capsule :
                    state.Kind == EntityKind.Minion ? PrimitiveType.Sphere :
                    state.Kind == EntityKind.Tower ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                visual = GameObject.CreatePrimitive(primitive);
                visual.AddComponent<PlaceholderVisual>();
            }
            visual.name = "Entity-" + state.Id + "-" + state.DisplayName;
            var handle = visual.GetComponent<EntityHandle>();
            if (handle == null) handle = visual.AddComponent<EntityHandle>();
            handle.EntityId = state.Id;
            if (visual.GetComponentInChildren<Collider>() == null)
                visual.AddComponent<CapsuleCollider>();
            if (organic != null) organic.DecorateEntity(visual, state);
            else art?.DecorateEntity(visual, state);
            if (state.Kind == EntityKind.Tower)
            {
                var danger = visual.AddComponent<TowerRangeVisual>();
                danger.Initialize(state, organic != null ? organic.ring : null);
            }
            return visual;
        }

        private static Vector3 Position(V2 pos, EntityKind kind)
        {
            var y = kind == EntityKind.Minion ? 0.8f : kind == EntityKind.Tower ? 2.1f :
                kind == EntityKind.Core ? 2.5f : 1.15f;
            return new Vector3(pos.X, y, pos.Y);
        }

        private static Vector3 Scale(EntityState entity)
        {
            if (entity.Kind == EntityKind.Minion) return Vector3.one * 1.25f;
            if (entity.Kind == EntityKind.Tower) return new Vector3(2.2f, 2.1f, 2.2f);
            if (entity.Kind == EntityKind.Core) return new Vector3(4.4f, 5f, 4.4f);
            if (entity.HeroId == "GKU_KID") return new Vector3(.95f, .8f, .95f);
            return entity.FormId == "cell_perfect" ? new Vector3(1.7f, 1.4f, 1.7f) :
                new Vector3(1.4f, 1.15f, 1.4f);
        }

        private static Color ColorFor(EntityState e)
        {
            if (e.Kind != EntityKind.Hero)
                return e.Team == Team.Blue ? new Color(0.24f, 0.68f, 0.94f) : new Color(0.95f, 0.36f, 0.38f);
            if (e.HeroId == "CELL_Z") return e.FormId == "cell_perfect" ? new Color(0.75f, 0.98f, 0.2f) : new Color(0.18f, 0.55f, 0.17f);
            if (e.FormId == "super_god") return new Color(1f, 0.2f, 0.3f);
            if (e.FormId == "super_blue") return new Color(0.2f, 0.7f, 1f);
            if (e.FormId == "super_blue_kk") return new Color(0.9f, 0.17f, 0.85f);
            if (e.FormId == "super_ui_sign") return new Color(0.72f, 0.8f, 0.95f);
            if (e.FormId == "super_ui") return new Color(0.98f, 0.98f, 1f);
            if (e.FormId == "kid_nimbus") return new Color(1f, 0.85f, 0.1f);
            if (e.FormId == "kid_mastery") return new Color(1f, 0.58f, 0.15f);
            return e.Team == Team.Blue ? new Color(0.96f, 0.51f, 0.13f) : new Color(0.7f, 0.18f, 0.65f);
        }

        private void RenderEvent(GameEvent e)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameplayValidationProbe.Observe(e, world);
#endif
            art?.Emit(e, world);
            if (views.TryGetValue(e.ActorId, out var actorView)) actorView.GetComponent<CharacterPresentation>()?.React(e);
            if (e.TargetId != e.ActorId && views.TryGetValue(e.TargetId, out var targetView)) targetView.GetComponent<CharacterPresentation>()?.React(e);
            if (art == null && e.Kind == EventKind.Ability)
                SpawnPulse(e.Position, new Color(0.3f, 0.75f, 1f), 1.5f);
            else if (art == null && e.Kind == EventKind.Transform)
                SpawnPulse(e.Position, Color.yellow, 3.3f);
            else if (art == null && e.Kind == EventKind.Death)
                SpawnPulse(e.Position, Color.red, 1.7f);
            if (e.ActorId == world.PlayerId || e.TargetId == world.PlayerId || e.Kind == EventKind.Victory)
            {
                if (e.Kind != EventKind.Damage && e.Kind != EventKind.Attack)
                {
                    lastMessage = e.Message;
                    messageUntil = Time.time + 3f;
                }
            }
        }

        private static void SpawnPulse(V2 location, Color color, float size)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "VisualPulse";
            sphere.transform.position = new Vector3(location.X, 0.5f, location.Y);
            sphere.transform.localScale = Vector3.one * size;
            var col = sphere.GetComponent<Collider>();
            if (col != null) Destroy(col);
            sphere.GetComponent<Renderer>().material.color = color;
            sphere.AddComponent<PulseEffect>();
        }

        private void BuildEnvironment()
        {
            if (environmentBuilt) return;
            environmentBuilt = true;
            CreateBlock("Solo", new Vector3(0, -0.55f, 0), new Vector3(72, 1, 88), new Color(0.10f, 0.19f, 0.15f));
            foreach (var x in new[] { -17, 0, 17 })
                CreateBlock("Rota", new Vector3(x, -0.015f, 0), new Vector3(7, 0.1f, 79), new Color(0.37f, 0.33f, 0.26f));
            CreateBlock("Rio Central", new Vector3(0, 0.015f, 0), new Vector3(68, 0.11f, 5), new Color(0.13f, 0.34f, 0.45f));
            foreach (var side in new[] { -1, 1 })
            {
                var x = side * 8.5f;
                foreach (var z in new[] { -30f, -12f, 12f, 30f })
                {
                    CreateBlock("Floresta", new Vector3(x, 1.1f, z), new Vector3(3.4f, 2.2f, 5.5f), new Color(0.09f, 0.31f, 0.15f));
                }
            }
            CreateBlock("Fonte Aliada", new Vector3(0, 0.05f, -35), new Vector3(12, 0.2f, 9), new Color(0.12f, 0.46f, 0.74f));
            CreateBlock("Fonte Inimiga", new Vector3(0, 0.05f, 35), new Vector3(12, 0.2f, 9), new Color(0.75f, 0.17f, 0.22f));
            // Decorative blocks have no colliders; the ground is always clickable.
            foreach (var dec in decorations)
            {
                if (dec.name == "Solo") continue;
                var collider = dec.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
            }
            art?.BuildEnvironment(decorations);
        }

        private void CreateBlock(string label, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = label;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material.color = color;
            decorations.Add(go);
        }

        private void CreateCamera()
        {
            if (gameCamera != null) return;
            var cameraObject = new GameObject("Isometric Camera");
            cameraObject.tag = "MainCamera";
            gameCamera = cameraObject.AddComponent<Camera>();
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 20f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.09f, 0.12f, 0.17f);
            cameraObject.AddComponent<AudioListener>();
            if (art != null) cameraObject.AddComponent<ArenaCameraController>();
            if (world == null)
            {
                cameraObject.transform.position = new Vector3(0, 40f, -30f);
                cameraObject.transform.rotation = Quaternion.Euler(55f, 0, 0);
            }
            else
            {
                var hero = world.Player;
                var focus = new Vector3(hero.Position.X, 0, hero.Position.Y);
                cameraObject.transform.position = focus + new Vector3(0, 31f, -20f);
                cameraObject.transform.LookAt(focus);
            }
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObject.transform.rotation = Quaternion.Euler(55f, -35f, 0);
        }

        private void UpdateCamera(EntityState hero)
        {
            if (gameCamera == null) return;
            var focus = new Vector3(hero.Position.X, 0, hero.Position.Y);
            var controller = gameCamera.GetComponent<ArenaCameraController>();
            if (controller != null) { controller.Tick(focus); return; }
            var targetCamera = focus + new Vector3(0, 31f, -20f);
            gameCamera.transform.position = Vector3.Lerp(gameCamera.transform.position, targetCamera, Time.deltaTime * 7f);
            gameCamera.transform.rotation = Quaternion.LookRotation(focus - gameCamera.transform.position, Vector3.up);
        }

        private void ReturnToSelection()
        {
            foreach (var go in views.Values) if (go != null) Destroy(go);
            views.Clear(); lastForm.Clear();
            world = null; shopOpen = false;
            lastMessage = "Selecione um personagem para iniciar.";
        }

        private void RestartMatch()
        {
            var id = world.Player.HeroId;
            foreach (var go in views.Values) if (go != null) Destroy(go);
            views.Clear(); lastForm.Clear();
            world = new GameWorld(id);
            RefreshViews();
            shopOpen = false;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = Color.white;
            normalStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            normalStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true };
            smallStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            var original = GUI.skin;
            var originalBackground = GUI.backgroundColor;
            if (art != null)
            {
                GUI.skin = art.Skin(original);
                // Kenney panels contain pale regions; keep white HUD text readable.
                GUI.backgroundColor = new Color(0.16f, 0.22f, 0.29f, 1f);
            }
            try { DrawGui(); if (art != null && world != null) art.DrawKeys(); }
            finally { GUI.skin = original; GUI.backgroundColor = originalBackground; }
        }

        private void DrawGui()
        {
            EnsureStyles();
            if (world == null)
            {
                var width = Mathf.Min(620, Screen.width - 30);
                var left = (Screen.width - width) * 0.5f;
                var top = Mathf.Max(16, (Screen.height - 430) * 0.5f);
                GUI.Box(new Rect(left, top, width, 430), "");
                GUI.Label(new Rect(left+12, top+12, width-24, 45), "ESCOLHA SEU GUERREIRO", titleStyle);
                GUI.Label(new Rect(left+24, top+55, width-48, 32),
                    "Selecione um personagem, conheca seu estilo e confirme antes da partida.", smallStyle);
                var heroes = new[] { "GKU_KID", "GKU_SUPER", "CELL_Z" };
                var roles = new[] { "Lutador agil | corpo a corpo", "Guerreiro de Ki | longa distancia",
                    "Tanque / sustentacao | corpo a corpo" };
                for (var i=0; i<3; ++i)
                {
                    var id = heroes[i];
                    var def = Catalog.Hero(id);
                    var y = top + 94 + i*83;
                    var selected = selectedChampionId == id;
                    GUI.Box(new Rect(left+22,y,width-44,78),
                        (selected ? "✓ " : "") + def.Name + "  |  " + roles[i]);
                    GUI.Label(new Rect(left+42,y+29,width-205,42),
                        "HP " + (int)def.MaxHp + "  •  Ki " + (int)def.MaxKi +
                        "  •  Ataque " + (int)def.BaseAttack, smallStyle);
                    if (GUI.Button(new Rect(left+width-170,y+30,128,35),
                        selected ? "Selecionado" : "Selecionar"))
                        selectedChampionId = id;
                }
                GUI.Label(new Rect(left+25,top+350,width-50,26),
                    "Selecionado: " + Catalog.Hero(selectedChampionId).Name, normalStyle);
                if (GUI.Button(new Rect(left+width-225,top+378,203,40), "CONFIRMAR E JOGAR"))
                    StartMatch(selectedChampionId);
                return;
            }
            var hero = world.Player;
            DrawHealthLabels();
            GUI.Box(new Rect(12, 10, 340, 170), "");
            GUI.Label(new Rect(24, 18, 310, 25), hero.DisplayName + " | Nv. " + hero.Level + " | " + hero.FormName, normalStyle);
            DrawBar(new Rect(25, 50, 305, 19), hero.Hp / hero.MaxHp, new Color(0.1f, 0.72f, 0.26f), "HP " + (int)hero.Hp + "/" + (int)hero.MaxHp);
            DrawBar(new Rect(25, 75, 305, 19), hero.Ki / hero.MaxKi, new Color(0.15f, 0.46f, 0.98f), "Ki " + (int)hero.Ki + "/" + (int)hero.MaxKi);
            var exp = hero.Level >= 18 ? 1 : hero.Experience / Catalog.ExperienceForNextLevel(hero.Level);
            DrawBar(new Rect(25, 100, 305, 15), exp, new Color(0.92f, 0.78f, 0.13f), hero.Level >= 18 ? "NIVEL MAXIMO" : "XP " + (int)hero.Experience);
            GUI.Label(new Rect(25, 122, 310, 22),
                "Zeni: " + hero.Zeni + " | ATQ: " + (int)hero.PhysicalAttack +
                " | ARM: " + (int)hero.Armor + " | Regen: " +
                (hero.HpRegenPer5 / 5f).ToString("0.0") + "/s", smallStyle);
            GUI.Label(new Rect(25, 141, 310, 25), !hero.Alive ? "DERROTADO - retorna em " + Mathf.CeilToInt(Mathf.Max(0, hero.RespawnTime)) + "s" : hero.Charging ? (hero.Ki >= hero.MaxKi ?
                    "TREINO: +" + hero.TrainingXpPerSecond.ToString("0.0") + " XP/s • vulneravel" :
                    "CARREGANDO KI • vulneravel") : "C: carregar Ki • P: loja • H: ajuda", smallStyle);
            GUI.Box(new Rect(Screen.width * 0.5f - 220, Screen.height - 150, 440, 137),
                "HABILIDADES  |  Pontos: " + hero.UnspentSkillPoints + "  |  Ctrl+Q/W/E/R");
            for (var i = 0; i < 4; ++i)
            {
                var name = new[] { "Q Rajada", "W Impulso", "E Escudo", "R Explosao" }[i];
                var slot = (AbilitySlot)i;
                var rank = hero.SkillRanks[i];
                var ready = rank == 0 ? "Bloqueada" :
                    hero.SkillCooldown[i] <= 0.01f ? "Pronto" : hero.SkillCooldown[i].ToString("0.0") + "s";
                var x = Screen.width * 0.5f - 206 + i * 105;
                GUI.Box(new Rect(x, Screen.height - 80, 100, 58),
                    name + "\nNivel " + rank + "/" + (i == 3 ? 3 : 5) + "\n" + ready);
                if (world.CanUpgradeAbility(hero, slot) &&
                    GUI.Button(new Rect(x + 76, Screen.height - 107, 24, 24), "+"))
                    Upgrade(slot);
            }
            GUI.Box(new Rect(Screen.width - 220, 12, 206, 89), "PARTIDA");
            GUI.Label(new Rect(Screen.width - 208, 38, 188, 55), "Tempo: " + world.Elapsed.ToString("0") + "s\nTropas e torres nas 3 rotas", smallStyle);
            if (Time.time < messageUntil)
                GUI.Box(new Rect(Screen.width * 0.5f - 240, 16, 480, 42), lastMessage);
            if (showHelp)
            {
                GUI.Box(new Rect(12, Screen.height - 218, 350, 198), "CONTROLES (H para ocultar)");
                GUI.Label(new Rect(25, Screen.height - 189, 326, 165),
                    "Direito: mover/atacar 1x | S: parar\nQWER: tecnicas | Ctrl+QWER: evoluir\nC: Ki e treino XP | P: loja e receitas\nScroll: zoom | bordas/setas/meio: camera\nEspaco: centralizar | Y: seguir/livre\nEsc: selecao | Enter: reiniciar", smallStyle);
            }
            if (shopOpen) ShopPanel(hero);
            if (world.Winner.HasValue) EndPanel();
        }

        private static readonly string[] ShopCategories =
            { "Todos", "Ataque", "Ki", "Defesa", "Movimento" };

        // Procedural, original in-game icon: recognizable by category without
        // redistributing protected third-party item illustrations.
        private Texture2D ItemIcon(ItemDefinition item)
        {
            if (itemIcons.TryGetValue(item.Id, out var existing)) return existing;
            const int size = 32;
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Point;
            var baseColor = item.Category == "Ataque" ? new Color(.6f,.19f,.15f) :
                item.Category == "Defesa" ? new Color(.15f,.49f,.36f) :
                item.Category == "Movimento" ? new Color(.48f,.32f,.14f) :
                new Color(.18f,.38f,.69f);
            var seed = item.Id.Length * 13;
            foreach (var c in item.Id) seed = (seed * 31 + c) & 0x7fffffff;
            for (var y=0; y<size; ++y)
            for (var x=0; x<size; ++x)
            {
                var dx=x-15.5f; var dy=y-15.5f;
                var core = item.Category == "Ataque" ? Mathf.Abs(dx+dy*.36f)<3.4f &&
                    Mathf.Abs(dy)<12 : item.Category == "Defesa" ?
                    Mathf.Abs(dx)<(12f-Mathf.Abs(dy)*.65f) && dy>-12 && dy<11 :
                    item.Category == "Movimento" ? dy<6 && dy>-6 && dx>-11 && dx<13 :
                    dx*dx+dy*dy<105;
                var border = x<2||y<2||x>29||y>29;
                var pixel = border ? Color.black :
                    core ? Color.Lerp(Color.white, new Color(1f,.82f,.47f), (seed%7)*.08f) :
                    baseColor * (.7f+(x+y)/128f);
                result.SetPixel(x,y,pixel);
            }
            result.Apply();
            itemIcons[item.Id] = result;
            return result;
        }

        private void ShopPanel(EntityState hero)
        {
            var width = Mathf.Min(410f, Screen.width * .48f);
            var left = Screen.width - width - 12f;
            var height = Mathf.Min(Screen.height - 120f, 550f);
            GUI.Box(new Rect(left, 110, width, height), "LOJA • Zeni " + hero.Zeni +
                " • Comprar somente na base");
            var selected = Array.IndexOf(ShopCategories, shopCategory);
            if (selected < 0) selected = 0;
            if (GUI.Button(new Rect(left+12, 140, width-24, 31),
                "Categoria: " + shopCategory + "   (clique para alterar)"))
                shopCategory = ShopCategories[(selected+1)%ShopCategories.Length];
            var count = 0;
            foreach (var item in Catalog.AllItems)
                if (shopCategory == "Todos" || item.Category == shopCategory) ++count;
            var viewHeight = Mathf.Max(90, height - 223f);
            shopScroll = GUI.BeginScrollView(new Rect(left+9, 176, width-18, viewHeight),
                shopScroll, new Rect(0, 0, width - 40, count*67 + 4));
            var top = 1f;
            foreach (var item in Catalog.AllItems)
            {
                if (shopCategory != "Todos" && item.Category != shopCategory) continue;
                GUI.DrawTexture(new Rect(4, top+5, 43, 43), ItemIcon(item));
                GUI.Label(new Rect(54, top, width-162, 36), item.Name + "\n" +
                    item.Description, smallStyle);
                var price = world.RemainingItemCost(hero,item.Id);
                GUI.Label(new Rect(54, top+38, width-158, 20),
                    price + " Zeni" + (item.Components.Length>0 ? " • receita" : ""), smallStyle);
                if (GUI.Button(new Rect(width-102,top+7,77,27), "Comprar"))
                {
                    var ok = world.Submit(new GameCommand { ActorId=hero.Id,
                        Kind=CommandKind.Buy, ItemId=item.Id });
                    lastMessage = ok ? "Equipou " + item.Name :
                        "Compra negada: base, saldo, slots ou item unico.";
                    messageUntil = Time.time + 3f;
                }
                top += 67;
            }
            GUI.EndScrollView();
            var inventoryY = 185 + viewHeight;
            GUI.Label(new Rect(left+14,inventoryY,width-28,22),
                "INVENTARIO (clique Vender para receber 70% do valor)", smallStyle);
            for (var i=0; i<Mathf.Min(6,hero.Inventory.Count); ++i)
            {
                var item = Catalog.Item(hero.Inventory[i]);
                if (item == null) continue;
                var x = left+11+(i%3)*((width-22)/3f);
                var y = inventoryY+24+(i/3)*35;
                GUI.DrawTexture(new Rect(x,y+3,23,23),ItemIcon(item));
                if (GUI.Button(new Rect(x+26,y,((width-22)/3f)-28,29),
                    "Vender"))
                {
                    if (world.Submit(new GameCommand { ActorId=hero.Id,
                        Kind=CommandKind.Sell, ItemId=item.Id })) break;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var icon in itemIcons.Values) if (icon != null) Destroy(icon);
            itemIcons.Clear();
        }

        private void EndPanel()
        {
            var w = 440f;
            var left = Screen.width * 0.5f - w * 0.5f;
            var top = Screen.height * 0.5f - 130;
            GUI.Box(new Rect(left, top, w, 260), "");
            var victory = world.Winner == Team.Blue;
            GUI.Label(new Rect(left + 20, top + 30, w - 40, 55), victory ? "VITORIA" : "DERROTA", titleStyle);
            GUI.Label(new Rect(left + 22, top + 102, w - 44, 40), "O nucleo da equipe adversaria foi destruido.", normalStyle);
            if (GUI.Button(new Rect(left + 60, top + 170, w - 120, 52), "Jogar novamente (Enter)")) RestartMatch();
        }

        private void DrawHealthLabels()
        {
            if (gameCamera == null) return;
            foreach (var e in world.Entities)
            {
                if (!e.Alive || (e.Kind != EntityKind.Hero && e.Kind != EntityKind.Tower && e.Kind != EntityKind.Core)) continue;
                var p = gameCamera.WorldToScreenPoint(Position(e.Position, e.Kind) + Vector3.up * (e.Kind == EntityKind.Hero ? 1.6f : 2.4f));
                if (p.z < 0) continue;
                var rect = new Rect(p.x - 54, Screen.height - p.y - 12, 108, 9);
                if (rect.x < 0 || rect.x > Screen.width || rect.y < 0 || rect.y > Screen.height) continue;
                if (e.Kind == EntityKind.Hero)
                    GUI.Label(new Rect(rect.x-30, rect.y-37, rect.width+60, 24),
                        e.DisplayName + " Nv." + e.Level, smallStyle);
                DrawBar(rect, e.Hp / e.MaxHp, e.Team == Team.Blue ? Color.green : Color.red, "");
                if (e.Shield > 0)
                    DrawBar(new Rect(rect.x,rect.y+10,rect.width,4),e.Shield/e.MaxHp,
                        new Color(.37f,.71f,1f),"");
            }
        }

        private void DrawBar(Rect rect, float fraction, Color fill, string label)
        {
            GUI.Box(rect, "");
            var original = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(fraction), rect.height - 4), art != null && art.bar != null ? art.bar : Texture2D.whiteTexture);
            GUI.color = original;
            if (!string.IsNullOrEmpty(label)) GUI.Label(rect, label);
        }
    }

    // World-space tower radius, shown only while the local hero is near an enemy tower.
    // Visual only: targeting and distance checks remain authoritative in GameWorld.
    public sealed class TowerRangeVisual : MonoBehaviour
    {
        private LineRenderer circle;
        private Material ownedMaterial;

        public void Initialize(EntityState turret, Material sharedMaterial)
        {
            var root = new GameObject("Tower range warning");
            root.transform.SetParent(transform, false);
            circle = root.AddComponent<LineRenderer>();
            if (sharedMaterial == null)
            {
                ownedMaterial = new Material(Shader.Find("Sprites/Default"));
                circle.sharedMaterial = ownedMaterial;
            }
            else circle.sharedMaterial = sharedMaterial;
            circle.useWorldSpace = true;
            circle.loop = true;
            circle.positionCount = 96;
            circle.startWidth = circle.endWidth = 0.075f;
            circle.startColor = circle.endColor = turret.Team == Team.Red
                ? new Color(1f, 0.32f, 0.20f, 0.9f)
                : new Color(0.25f, 0.7f, 1f, 0.9f);
            for (var i = 0; i < circle.positionCount; ++i)
            {
                var theta = i * 2f * Mathf.PI / circle.positionCount;
                circle.SetPosition(i, new Vector3(turret.Position.X + Mathf.Cos(theta) * turret.AttackRange,
                    0.16f, turret.Position.Y + Mathf.Sin(theta) * turret.AttackRange));
            }
            circle.enabled = false;
        }

        public void Show(bool visible)
        {
            if (circle != null) circle.enabled = visible;
        }

        private void OnDestroy()
        {
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }

    public sealed class PlaceholderVisual : MonoBehaviour { }

    public sealed class EntityHandle : MonoBehaviour
    {
        public int EntityId;
    }

    public sealed class PulseEffect : MonoBehaviour
    {
        private float lifespan = 0.42f;
        private void Update()
        {
            lifespan -= Time.deltaTime;
            transform.localScale += Vector3.one * (Time.deltaTime * 5f);
            if (lifespan <= 0) Destroy(gameObject);
        }
    }
}

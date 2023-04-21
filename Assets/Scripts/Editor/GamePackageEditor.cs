using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Cinemachine;

[CustomEditor(typeof(GamePackage))]
[CanEditMultipleObjects]
public class GamePackageEditor : Editor
{
    string m_messageLog;
    string m_warningLog;
    string m_errorLog;

    bool isSceneGameManagerDirty = false;
    bool isSceneCameraDirty = false;

    GameManager sceneGameManager = null;
    CinemachineBrain sceneBrain = null;
    CinemachineVirtualCamera sceneVirtCam = null;

    List<ObjectInitialiser> m_objInitialisers = new List<ObjectInitialiser>();

    private void OnEnable()
    {
        m_objInitialisers.Clear();
        m_objInitialisers.Add(new PlayerReferenceInitialiser("m_player", "Player"));
        m_objInitialisers.Add(new GameManagerInitialiser("m_payloadPath", "Payload Path", typeof(PayloadSpline)));
        m_objInitialisers.Add(new PayloadControllerInitialiser("m_payload", "Payload"));

        m_objInitialisers.Add(new ObjectInitialiser("m_spawnZone", "Spawn Zone", typeof(SpawnZone)));
        m_objInitialisers.Add(new SpawnControllerInitialiser("m_spawner", "Spawn Controller"));
    }

    internal class ObjectInitialiser
    {
        internal bool isDirty = false;
        internal string propertyName;
        internal string friendlyObjectName;

        internal Object sceneObject;
        internal System.Type objectType;

        internal ObjectInitialiser(string propertyName, string friendlyObjectName, System.Type objectType)
        {
            this.propertyName = propertyName;
            this.friendlyObjectName = friendlyObjectName;
            this.objectType = objectType;
        }

        internal void CreateButton(GamePackageEditor editor)
        {
            //if (editor.SerializedCreateButton("Create " + friendlyObjectName, propertyName, out sceneObject))
            //{
            //    isDirty = true;
            //}

            if (GUILayout.Button("Create " + friendlyObjectName))
            {
                Create(editor);
            }
        }

        internal virtual void Create(GamePackageEditor editor)
        {
            if(editor.SerializedCreate(propertyName, out sceneObject))
            {
                isDirty = true;
            }
        }

        internal bool ObjectExistsInScene(out Object sceneObject)
        {
            sceneObject = FindObjectOfType(objectType);
            return sceneObject != null;
        }

        internal bool ObjectExistsInScene()
        {
            return ObjectExistsInScene(out Object sceneObject);
        }

        internal void ApplyChanges()
        {
            ApplySceneChangesToSceneObject(sceneObject, isDirty);
            isDirty = false;
        }
    }

    internal class GameManagerInitialiser : ObjectInitialiser
    {

        internal GameManagerInitialiser(string propertyName, string friendlyObjectName, System.Type objectType) : base(propertyName, friendlyObjectName, objectType)
        {
            
        }

        internal override void Create(GamePackageEditor editor)
        {
            if(editor.SerializedAddSingletonObjectToScene(propertyName, out sceneObject))
            {
                isDirty = true;
            }

            if (sceneObject != null)
            {
                // Scene object was found but not changed.
                GameManager sceneGameManager = editor.sceneGameManager;
                if (sceneGameManager == null)
                {
                    sceneGameManager = FindObjectOfType<GameManager>();
                }
                if (editor.AttachObjectToGameManager(sceneGameManager, sceneObject, propertyName))
                {
                    editor.isSceneGameManagerDirty = true;
                    editor.SmartLog("GameManager was found in scene. Adding " + friendlyObjectName + " reference to GameManager.\n");
                }
            }
        }

        internal bool FindAndAttachToGameManager(GamePackageEditor editor, GameManager sceneGameManager)
        {
            return editor.FindObjectToAttachToGameManager(sceneGameManager, propertyName, objectType);
        }
    }

    internal class PlayerReferenceInitialiser : GameManagerInitialiser
    {
        internal PlayerReferenceInitialiser(string propertyName, string friendlyObjectName) : base(propertyName, friendlyObjectName, typeof(PlayerReference))
        {

        }

        internal override void Create(GamePackageEditor editor)
        {
            base.Create(editor);

            if (sceneObject != null)
            {
                var scenePlayer = sceneObject as PlayerReference;

                if(FindAndAttachVirtualCamera(editor, scenePlayer))
                {
                    isDirty = true;
                }
            }
        }

        bool FindAndAttachVirtualCamera(GamePackageEditor editor, PlayerReference scenePlayer)
        {
            CinemachineVirtualCamera sceneVirtCam = FindObjectOfType<CinemachineVirtualCamera>();
            if (sceneVirtCam == null)
            {
                // Did not find spawnZone in Scene
                editor.m_warningLog += "Could not find a virtual Camera in Scene. Ensure that CinemachineVirtualCamera will Follow the PlayerCameraTarget.\n";
                return false;
            }

            editor.m_messageLog += "Found Virtual Camera(" + sceneVirtCam.name + ") in Scene. Using " + sceneObject.name + "'s Camera Target as the Virtual Camera's follow Target.\n";

            SerializedObject virtCamSerializedObject = new SerializedObject(sceneVirtCam);
            var followProperty = virtCamSerializedObject.FindProperty("m_Follow");
            followProperty.objectReferenceValue = scenePlayer.cameraController.cinemachineCameraTarget;

            virtCamSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(sceneVirtCam);

            return true;
        }
    }

    internal class SpawnControllerInitialiser : GameManagerInitialiser
    {
        internal SpawnControllerInitialiser(string propertyName, string friendlyObjectName) : base(propertyName, friendlyObjectName, typeof(EnemySpawnController))
        {

        }

        internal override void Create(GamePackageEditor editor)
        {
            //if (editor.SerializedCreate(propertyName, out sceneObject))
            //{
            //    isDirty = true;
            //}

            base.Create(editor);

            if(sceneObject != null)
            {
                SpawnZone sceneZone = FindObjectOfType<SpawnZone>();
                if (sceneZone == null)
                {
                    // Did not find spawnZone in Scene
                    editor.m_warningLog += "Could not find a spawn zone in Scene. Ensure that EnemySpawnControllers have a SpzwnZone Reference.\n";
                    return;
                }

                editor.m_messageLog += "Found Spawn Zone(" + sceneZone.name + ") in Scene. Using as EnemySpawnController's target zone.\n";

                SerializedObject spawnControllerSerializedObject = new SerializedObject(sceneObject);
                var playerProperty = spawnControllerSerializedObject.FindProperty("m_spawnZone");
                playerProperty.objectReferenceValue = sceneZone;

                spawnControllerSerializedObject.ApplyModifiedProperties();
                isDirty = true;
            }
        }
    }

    internal class PayloadControllerInitialiser : GameManagerInitialiser
    {
        internal PayloadControllerInitialiser(string propertyName, string friendlyObjectName) : base(propertyName, friendlyObjectName, typeof(PayloadController))
        {

        }

        internal override void Create(GamePackageEditor editor)
        {
            base.Create(editor);

            if (sceneObject != null)
            {
                var scenePayload = sceneObject as PayloadController;

                var sceneSpline = FindObjectOfType<PayloadSpline>();
                if(sceneSpline != null)
                {
                    scenePayload.SetPath(sceneSpline);
                    editor.m_messageLog += "PayloadSpline was found in Scene. Adding as reference to payload.\n";
                    isDirty = true;
                }
                else
                {
                    editor.m_warningLog += "PayloadSpline was not found in Scene. Payload Controller MUST have a reference to a Payload Spline.\n";
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BeginUI();

        CreateDefaultGameScene();
        SeperateButtons();

        EndUI();
    }

    void BeginUI()
    {
        GUILayout.Label("DEBUG");

        m_messageLog = null;
        m_warningLog = null;
        m_errorLog = null;
    }

    void EndUI()
    {
        for (int i = 0; i < m_objInitialisers.Count; i++)
        {
            m_objInitialisers[i].ApplyChanges();
        }

        ApplySceneChangesToSceneObject(sceneGameManager, isSceneGameManagerDirty);
        isSceneGameManagerDirty = false;
        sceneGameManager = null;

        ApplySceneChangesToSceneObject(sceneBrain, isSceneCameraDirty);
        ApplySceneChangesToSceneObject(sceneVirtCam, isSceneCameraDirty);
        isSceneCameraDirty = false;
        sceneBrain = null;
        sceneVirtCam = null;

        PrintLogs();
    }

    void CreateDefaultGameScene()
    {
        if(!GUILayout.Button("Create Default Scene"))
        {
            return;
        }

        // GameManager
        if(SerializedAddSingletonObjectToScene("m_gameManager", out sceneGameManager))
        {
            isSceneGameManagerDirty = true;
        }

        for (int i = 0; i < m_objInitialisers.Count; i++)
        {
            var gmInitialiser = m_objInitialisers[i] as GameManagerInitialiser;
            if(gmInitialiser != null)
            {
                gmInitialiser.FindAndAttachToGameManager(this, sceneGameManager);
            }
        }

        // Camera
        if (CreateCamera(out sceneBrain, out sceneVirtCam))
        {
            SmartLog("Camera is Created.\n");
            isSceneCameraDirty = true;
        }

        for (int i = 0; i < m_objInitialisers.Count; i++)
        {
            if(!m_objInitialisers[i].ObjectExistsInScene())
            {
                // Object not created yet
                m_objInitialisers[i].Create(this);
            }
            else
            {
                // Object already exists
                m_warningLog += m_objInitialisers[i].objectType.ToString() + " already exists in Scene.\n";
            }
        }
    }

    void SeperateButtons()
    {
        GUILayout.Label("Objects");
        // GameManager
        if (SerializedCreateSingletonButton("Create GameManager", "m_gameManager", out sceneGameManager))
        {
            isSceneGameManagerDirty = true;
        }

        if (FindObjectToAttachToGameManager(sceneGameManager, "m_player", typeof(PlayerReference)))
        {
            SmartLog("Player was found in scene. Adding Player reference to GameManager.\n");
            isSceneGameManagerDirty = true;
        }

        for (int i = 0; i < m_objInitialisers.Count; i++)
        {
            var gmInitialiser = m_objInitialisers[i] as GameManagerInitialiser;
            if (gmInitialiser != null)
            {
                gmInitialiser.FindAndAttachToGameManager(this, sceneGameManager);
            }
        }

        // Camera
        if (CameraCreateButton(out sceneBrain, out sceneVirtCam))
        {
            isSceneCameraDirty = true;
        }

        for (int i = 0; i < m_objInitialisers.Count; i++)
        {
            m_objInitialisers[i].CreateButton(this);
        }
    }

    #region Generic
    bool SerializedCreateSingletonButton<T>(string label, string propertyPath, out T sceneObject) where T : Object
    {
        if (GUILayout.Button(label))
        {
            return SerializedAddSingletonObjectToScene(propertyPath, out sceneObject);
        }
        else
        {
            // No object was created or found.
            sceneObject = null;
        }

        return false;
    }

    bool SerializedCreateButton<T>(string label, string propertyPath, out T sceneObject) where T : Object
    {
        if (GUILayout.Button(label))
        {
            return SerializedCreate(propertyPath, out sceneObject);
        }
        else
        {
            // No object was created or found.
            sceneObject = null;
        }

        return false;
    }

    bool SerializedCreate<T>(string propertyPath, out T sceneObject) where T : Object
    {
        var objectProperty = serializedObject.FindProperty(propertyPath);
        T refValue = objectProperty.objectReferenceValue as T;

        if (refValue == null)
        {
            m_errorLog += propertyPath + " is null in game Package.\n";
            sceneObject = null;
            return false;
        }

        sceneObject = AddObjectToScene(refValue);
        return sceneObject != null;
    }

    bool SerializedAddSingletonObjectToScene<T>(string propertyPath, out T sceneObject) where T : Object
    {
        var objectProperty = serializedObject.FindProperty(propertyPath);
        T refValue = objectProperty.objectReferenceValue as T;

        return AddSingletonObjectToScene(refValue, out sceneObject);
    }

    bool AddSingletonObjectToScene<T>(T singletonPrefab, out T singleton) where T : Object
    {
        if(singletonPrefab == null)
        {
            m_errorLog += typeof(T).ToString() + " is null in game Package.\n";
            singleton = null;
            return false;
        }

        singleton = FindObjectOfType(singletonPrefab.GetType()) as T;
        if (singleton == null)
        {
            //singleton = Instantiate(singletonPrefab);
            singleton = AddObjectToScene(singletonPrefab);
            return true;
        }
        else
        {
            m_warningLog += typeof(T).ToString() + " already exists in Scene.\n";
            //Debug.LogWarning(typeof(T).ToString() + " already exists in Scene.");
            EditorGUIUtility.PingObject(singleton);
            return false;
        }
    }

    T AddObjectToScene<T>(T objectPrefab) where T : Object
    {
        T sceneObject = PrefabUtility.InstantiatePrefab(objectPrefab) as T;
        EditorGUIUtility.PingObject(sceneObject);
        return sceneObject;
    }

    static void ApplySceneChangesToSceneObject(Object target, bool shouldBeDirty)
    {
        if (target != null && shouldBeDirty)
        {
            EditorUtility.SetDirty(target);
        }
    }
    #endregion // Generic

    #region PlayerGameManager
    bool AttachObjectToGameManager(GameManager sceneGameManager, Object sceneObject, string propertyName)
    {
        if(sceneGameManager == null || sceneObject == null)
        {
            return false;
        }

        SerializedObject gameManagerSerializedObject = new SerializedObject(sceneGameManager);
        var playerProperty = gameManagerSerializedObject.FindProperty(propertyName);
        playerProperty.objectReferenceValue = sceneObject;

        gameManagerSerializedObject.ApplyModifiedProperties();

        return true;
    }

    bool FindObjectToAttachToGameManager(GameManager sceneGameManager, string propertyName, System.Type objectType)
    {
        if (sceneGameManager == null)
        {
            return false;
        }

        var sceneObject = FindObjectOfType(objectType);
        if (sceneObject == null)
        {
            return false;
        }

        SerializedObject gameManagerSerializedObject = new SerializedObject(sceneGameManager);
        var playerProperty = gameManagerSerializedObject.FindProperty(propertyName);
        playerProperty.objectReferenceValue = sceneObject;

        gameManagerSerializedObject.ApplyModifiedProperties();

        return true;
    }
    #endregion // PlayerGameManager

    #region Camera
    bool ValidateCamera()
    {
        var package = target as GamePackage;

        if(package.ValidateCamera())
        {
            return true;
        }
        else
        {
            // Something Has gone wrong
            var camSetup = package.GetCameraSetup();
            m_errorLog += "CAMERA::CREATE::ERROR::";

            bool virtValid = camSetup.HasFlag(GamePackage.CameraBrainVirtualSetup.VirtualValid);
            if (!camSetup.HasFlag(GamePackage.CameraBrainVirtualSetup.BrainValid))
            {
                m_errorLog += "- CinemachineBrain is not present.";

                if (!virtValid)
                {
                    m_errorLog += " - ";
                }
            }

            if (!virtValid)
            {
                m_errorLog += "CinemachineVirtualCamera is not present. Try adding a Virtual Camera to the property field or as a component on the brain or it's child.";
            }

            m_errorLog += "\n";

            return false;
        }
    }

    bool CameraCreateButton(out CinemachineBrain sceneBrain, out CinemachineVirtualCamera sceneVirtCam)
    {
        if (GUILayout.Button("Create Camera"))
        {
            if (CreateCamera(out sceneBrain, out sceneVirtCam))
            {
                return true;
            }
        }
        sceneBrain = null;
        sceneVirtCam = null;
        return false;
    }

    bool CreateCamera(out CinemachineBrain sceneBrain, out CinemachineVirtualCamera sceneVirtCam)
    {
        sceneBrain = null;
        sceneVirtCam = null;
        if (!ValidateCamera())
        {
            return false;
        }

        // create brain
        var camBrainProperty = serializedObject.FindProperty("m_camera");
        var camBrainPrefab = camBrainProperty.objectReferenceValue as Cinemachine.CinemachineBrain;

        var camBrain = FindObjectOfType<Cinemachine.CinemachineBrain>();
        if (camBrain == null)
        {
            //singleton = Instantiate(singletonPrefab);
            camBrain = PrefabUtility.InstantiatePrefab(camBrainPrefab) as Cinemachine.CinemachineBrain;
            EditorGUIUtility.PingObject(camBrain);
            //return true;
        }
        else
        {
            m_warningLog += typeof(Cinemachine.CinemachineBrain).ToString() + " already exists in Scene.\n";
            //Debug.LogWarning(typeof(T).ToString() + " already exists in Scene.");
            EditorGUIUtility.PingObject(camBrain);
            return false;
        }

        // create virtual if needed
        var package = target as GamePackage;
        var camSetup = package.GetCameraSetup();

        sceneVirtCam = camBrain.GetComponentInChildren<CinemachineVirtualCamera>();

        if (camSetup.HasFlag(GamePackage.CameraBrainVirtualSetup.SerialisedVirtualValid) && !camSetup.HasFlag(GamePackage.CameraBrainVirtualSetup.ChildVirtualValid))
        {
            var virtCamProperty = serializedObject.FindProperty("m_virtualCamera");
            var virtCamPrefab = camBrainProperty.objectReferenceValue as CinemachineVirtualCamera;

            sceneVirtCam = AddObjectToScene(virtCamPrefab);
        }

        return true;
    }
    #endregion // Camera

    #region Spawner
    void AttachSpawnZoneToController(SpawnZone sceneSpawnZone, EnemySpawnController sceneSpawner)
    {
        sceneSpawner.SetSpawnZone(sceneSpawnZone);
    }
    #endregion

    #region Logs
    void PrintLogs()
    {
        if (m_messageLog != null)
        {
            Debug.Log(m_messageLog);
        }

        if (m_warningLog != null)
        {
            Debug.LogWarning(m_warningLog);
        }

        if (m_errorLog != null)
        {
            Debug.LogError(m_errorLog);
        }
    }

    // adds message to warningLog if warning log is not null
    void SmartLog(string message)
    {
        if (m_warningLog != null)
        {
            m_warningLog += message;
        }
        else
        {
            m_messageLog += message;
        }
    }
    #endregion // Logs
}

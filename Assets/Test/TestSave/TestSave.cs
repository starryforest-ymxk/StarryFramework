using UnityEngine;
using StarryFramework;
using UnityEngine.Events;

public class TestSave : MonoBehaviour
{
    readonly UnityAction action = new(() => Debug.Log("SaveData!!!"));

    private void OnEnable()
    {
        Framework.EventComponent.AddEventListener(FrameworkEvent.OnSaveData, action);
    }

    private void OnDisable()
    {
        Framework.EventComponent?.RemoveEventListener(FrameworkEvent.OnSaveData, action);
    }

    private void Update()
    {
        //Load
        if (Input.GetKeyUp(KeyCode.A))
        {
            Debug.Log("LoadData");
            Framework.SaveComponent.LoadData();
        }

        if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            Debug.Log("LoadData 0");
            Framework.SaveComponent.LoadData(0);
        }  
        
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Debug.Log("LoadData 1");
            Framework.SaveComponent.LoadData(1);
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Debug.Log("LoadData 2");
            Framework.SaveComponent.LoadData(2);
        }

        //Create New Game
        if (Input.GetKeyUp(KeyCode.B))
        {
            Debug.Log("New Game");
            Framework.SaveComponent.CreateNewData(true);
        }

        //Auto Save Data
        if (Input.GetKeyUp(KeyCode.Insert))
        {
            Debug.Log("Auto save PlayerData");
            Framework.SaveComponent.SaveData();
        }

        //Save Data manually
        if (Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Create new PlayerData");
            Framework.SaveComponent.CreateNewData(false);
        }

        if (Input.GetKeyUp(KeyCode.F10))
        {
            Debug.Log("Save PlayerData to PlayerData 0");
            Framework.SaveComponent.SaveData(0);
        }

        if (Input.GetKeyUp(KeyCode.F11))
        {
            Debug.Log("Save PlayerData to PlayerData 1");
            Framework.SaveComponent.SaveData(1);
        }

        if (Input.GetKeyUp(KeyCode.F12))
        {
            Debug.Log("Save PlayerData to PlayerData 2");
            Framework.SaveComponent.SaveData(2);
        }

        //Change content
        if (Input.GetKeyUp(KeyCode.D))
        {
            Debug.Log("PlayerData.test++");
            PlayerData playerData = Framework.SaveComponent.GetPlayerData<PlayerData>();
            if (playerData != null)
            {
                playerData.test++;
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            Debug.Log("Event1");
            Framework.EventComponent.InvokeEvent("event1");
        }

        //Change Save Info
        if (Input.GetKeyUp(KeyCode.S))
        {
            Debug.Log("Change save info to info 1");
            Framework.SaveComponent.SetSaveInfo(1);
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            Debug.Log("Change save info to \" auto test\"");
            Framework.SaveComponent.SetSaveInfo("auto test");
        }


        //Delete Data
        if (Input.GetKeyUp(KeyCode.F))
        {
            Debug.Log("Delete PlayerData 0");
            Framework.SaveComponent.DeleteData(0);
        }

        if (Input.GetKeyUp(KeyCode.F1))
        {
            Debug.Log("Delete PlayerData 1");
            Framework.SaveComponent.DeleteData(1);
        }

        if (Input.GetKeyUp(KeyCode.F2))
        {
            Debug.Log("Delete PlayerData 2");
            Framework.SaveComponent.DeleteData(2);
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            Debug.Log("Restart");
            Framework.ShutDown(ShutdownType.Restart);
        }

    }

}


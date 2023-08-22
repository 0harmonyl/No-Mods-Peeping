using BepInEx;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Utilla;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace No_Peeping_At_My_Mods
{
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.9")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        string newPrep;
        bool enabled = true;

        void Start()
        {
            Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            enabled = true;
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
            enabled = false;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            StartCoroutine(GetText());
            enabled = true;
        }

        void Update()
        {
            /* If the string we (might) currently be trying to get is empty then abort, else continue */
            if (newPrep == string.Empty) return;

            if(enabled)
            {
                if (GetPhotonViewFromVR(GorillaTagger.Instance.myVRRig.gameObject).Controller.CustomProperties.ContainsValue(newPrep))
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("mods", newPrep);
                    GetPhotonViewFromVR(GorillaTagger.Instance.myVRRig.gameObject).Controller.SetCustomProperties(hash);
                }
            }
        }

        IEnumerator GetText()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://pastebin.com/raw/xVeaHjYq");
            yield return www.SendWebRequest();

            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                // Show results as text
                newPrep = www.downloadHandler.text;
            }
        }

        private static PhotonView GetPhotonViewFromVR(GameObject vrRig)
        {
            MethodInfo getViewListMethod = AccessTools.Method(typeof(PhotonNetwork), "GetPhotonViewList");

            List<PhotonView> photonViews = (List<PhotonView>)getViewListMethod.Invoke(null, null);

            foreach (PhotonView photonView in photonViews)
            {
                Photon.Realtime.Player owner = (Photon.Realtime.Player)AccessTools.Field(typeof(PhotonView), "ownershipTransfer").GetValue(photonView);

                if (owner != null && owner.TagObject == vrRig)
                {
                    return photonView;
                }
            }

            return null;
        }
    }
}

using System;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public static NetworkingManager Instance { get; private set; }

    public const byte NetworkByte = ;   // Don't use ANY values that are 200 or more, those are integral networking bytes that you should NEVER use.
                                        // Try to find a value under 200 to use as your byte, some values to stay away from are anything under 10 and 51.
                                        // There are others, but I don't remember those.

    public Action<string, object[], VRRig> OnEventReceived;
    
    private void EventReceived(EventData photonEvent)
    {
        if (photonEvent.Code == NetworkByte)
        {
            if (photonEvent.Parameters.TryGetValue(ParameterCode.Data, out var rawData))
            {
                if (rawData is object[] arrayData)
                {
                    if (arrayData.Length == 0)
                        return;

                    string command = arrayData[0] as string;
                    object[] parameters = arrayData.Skip(1).ToArray();

                    VRRig sender = GorillaParent.instance.vrrigs
                        .FirstOrDefault(rig => rig.OwningNetPlayer.ActorNumber == photonEvent.Sender);

                    if (sender == null)
                        return;

                    OnEventReceived?.Invoke(command, parameters, sender);
                }
                else
                {
                    Debug.LogWarning($"[NetworkingManager] Unexpected data type: {rawData.GetType()}");
                }
            }
        }
    }
    
    public void SendEvent(string command, RaiseEventOptions options, object[] parameters)
    {
        if (!NetworkSystem.Instance.InRoom)
            return;

        PhotonNetwork.RaiseEvent(
            NetworkByte,
            new object[] { command }.Concat(parameters).ToArray(),
            options,
            SendOptions.SendReliable
        );
    }

    public void SendEvent(string command, ReceiverGroup receivers, params object[] parameters) =>
        SendEvent(command, new RaiseEventOptions() { Receivers = receivers }, parameters);

    public void SendEvent(string command, int[] targetActors, params object[] parameters) =>
        SendEvent(command, new RaiseEventOptions() { TargetActors = targetActors }, parameters);

    public void SendEvent(string command, int targetActor, params object[] parameters) =>
        SendEvent(command, new int[] { targetActor }, parameters);

    private void Start() => PhotonNetwork.NetworkingClient.EventReceived += EventReceived;
    private void Awake() => Instance = this;
}

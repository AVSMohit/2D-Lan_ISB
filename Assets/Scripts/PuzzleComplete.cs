using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public class PuzzleComplete : NetworkBehaviour
{
    HashSet<ulong> playersInTrigger= new HashSet<ulong>();

    public string nextScceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<ClientNetworkTransform>();
            if (player != null) 
            {
                playersInTrigger.Add(player.OwnerClientId);
                CheckAllPlayersInTrigger();   
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<ClientNetworkTransform>();
            if (player != null)
            {
                playersInTrigger.Remove(player.OwnerClientId);
                
            }
        }
    }

    void CheckAllPlayersInTrigger()
    {
        if (playersInTrigger.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            TriggerSceneTransitionServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerSceneTransitionServerRpc(ServerRpcParams rpcParams = default)
    {
        SceneTransitionManager.Instance.TransitionToScene(nextScceneName);  // Replace "NextScene" with your target scene name
    }


}

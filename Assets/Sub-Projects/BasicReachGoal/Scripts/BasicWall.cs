using UnityEngine;

public class BasicWall : MonoBehaviour {
    public BasicAgent agent;
    private void OnCollisionStay(Collision other) {
        if(other.transform.CompareTag("agent"))
        {
            agent.PunishHittingWall();
        }
    }

    private void OnCollisionEnter(Collision other) {
        if(other.transform.CompareTag("agent"))
        {
            agent.PunishHittingWall();
        }
    }
}
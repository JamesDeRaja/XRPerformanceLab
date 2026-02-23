using UnityEngine;

public class OverdrawToggle : MonoBehaviour
{
    [SerializeField] private GameObject overdrawRoot;

    public void OverdrawOn() { if (overdrawRoot) overdrawRoot.SetActive(true); }
    public void OverdrawOff() { if (overdrawRoot) overdrawRoot.SetActive(false); }
}
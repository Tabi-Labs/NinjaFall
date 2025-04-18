using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Timer : NetworkBehaviour
{
    [SerializeField]
    private float tiempo;
    [SerializeField]
    private float regresiveTime;
    public NetworkVariable<bool> timeStarted = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField]
    private TextMeshProUGUI GUI;
    //[SerializeField]
    //private TextMeshProUGUI GUI2;
    [SerializeField]
    private TextMeshProUGUI preGUI;
    [SerializeField]
    private float max;
    [SerializeField]
    private Image fill;
    //[SerializeField]
    //private Image fill2;
    private bool regresiveTimerFinished = false;
    private Vector3 originalScale;
    private Vector3 scaleTo;
    //[SerializeField]
    //private LocalizedStringTable LocalizedStringTable;

    public EventHandler OnRegresiveTimerFinished;

    //[SerializeField] RestaurantManager restaurantManager;


    private void Start()
    {
        //tiempo = 3.0f;
        timeStarted.Value = false;
        originalScale = GUI.transform.localScale;
        scaleTo = originalScale * 1.5f;
    }

    public override void OnNetworkSpawn()
    {
        timeStarted.OnValueChanged += ComprobarTimeStarted;
    }
    public override void OnNetworkDespawn()
    {
        timeStarted.OnValueChanged -= ComprobarTimeStarted;
    }
    private void ComprobarTimeStarted(bool previousValue, bool newValue)
    {
        timeStarted.Value = newValue;
    }

    private void FixedUpdate()
    {
        if (!timeStarted.Value) { return; }
        if (!regresiveTimerFinished) { return; }
        timerUpdate();
    }
    private void timerUpdate()
    {
        tiempo -= Time.fixedDeltaTime;
        GUI.text = ((int)tiempo).ToString();
        //GUI2.text = ((int)tiempo).ToString();
        fill.fillAmount = tiempo / max;
        //fill2.fillAmount = tiempo / max;
        if (tiempo <= 0)
        {
            tiempo = 0;
            if (IsServer)
            {
                timeStarted.Value = false;
                //restaurantManager.SaveMatchToDB();
            }
        }
    }
    private IEnumerator RunTimer()
    {

        while (regresiveTime > 0f)
        {
            preGUI.text = (regresiveTime).ToString();
            OnScale(preGUI.transform);
            yield return new WaitForSeconds(1.3f); // Esperar un segundo antes de continuar
            regresiveTime -= 1f; // Reducir el tiempo restante
        }
        if (regresiveTime == 0f)
        {
            //preGUI.text = LocalizedStringTable.GetTable().GetEntry("PreCountdown").GetLocalizedString();
            OnScale(preGUI.transform);
            yield return new WaitForSeconds(1.3f);
            regresiveTime -= 1f; // Reducir el tiempo restante
        }
        preGUI.text = "";
        regresiveTimerFinished = true;
        OnRegresiveTimerFinished?.Invoke(this, EventArgs.Empty);
    }
    private void OnScale(Transform gui)
    {
        gui.DOScale(scaleTo, 0.5f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                gui.DOScale(originalScale, 0.5f)
                .SetEase(Ease.InOutSine);
            });
    }
    public void ChangeTimeVariable()
    {
        if(NetworkManager)
        {
            timeStarted.Value = true;
            StartRegresiveCountdownClientRPC();
        }
        else
        {
            timeStarted.Value = true;
            StartCoroutine(RunTimer());
        }

    }
    [ClientRpc]
    private void StartRegresiveCountdownClientRPC()
    {
        StartCoroutine(RunTimer());
    }
}

/**
 * @file ipso objects definition
 *
 */
public class IPSO
{
    //Objects
    //oma-lable Objects Produced by OMA
    
    public const int IPSO_OBJECT_LWM2M_Security          =0;
    public const int IPSO_OBJECT_LWM2M_Server            =1;
    public const int IPSO_OBJECT_AccessControl           =2;
    public const int IPSO_OBJECT_Device                  =3;
    public const int IPSO_OBJECT_Connectivity_Monitoring =4;
    public const int IPSO_OBJECT_Firmware                =5;
    public const int IPSO_OBJECT_Location                =6;
    public const int IPSO_OBJECT_ConnectivityStatistics  =7;
    public const int IPSO_OBJECT_LockAndWipe             =8;
    public const int IPSO_OBJECT_SofwareManagement       =9;
    public const int IPSO_OBJECT_CellularConnectivity	=10;
    public const int IPSO_OBJECT_APNconnectionprofile    =11;
    public const int IPSO_OBJECT_WLANconnectivity        =12;
    public const int IPSO_OBJECT_Bearerselection         =13;
    public const int IPSO_OBJECT_SoftwareComponent       =14;
    public const int IPSO_OBJECT_DevCapMgmt              =15;
    public const int IPSO_OBJECT_Porfolio                =16;
    public const int IPSO_OBJECT_CommunicationsCharacteristics          =17;
    public const int IPSO_OBJECT_NASConfiguration        =18;
    public const int IPSO_OBJECT_LwM2MAPPDATA            =19;
    public const int IPSO_OBJECT_EventLog                =20;
    public const int IPSO_OBJECT_Pending                 =21;

    //ext-label Objects Produced by 3rd party Standards Development Organizations
    public const int IPSO_OBJECT_CmdhPolicy              =2048;
    public const int IPSO_OBJECT_ActiveCmdhPolicy        =2049;
    public const int IPSO_OBJECT_CmdhDefaults            =2050;
    public const int IPSO_OBJECT_CmdhDefEcValues         =2051;
    public const int IPSO_OBJECT_CmdhEcDefParamValues    =2052;
    public const int IPSO_OBJECT_CmdhLimits              =2053;
    public const int IPSO_OBJECT_CmdhNetworkAccessRules  =2054;
    public const int IPSO_OBJECT_CmdhNwAccessRule        =2055;
    public const int IPSO_OBJECT_CmdhBuffer              =2056;
    public const int IPSO_OBJECT_CmdhBackOffParametersSet =2057;
    public const int IPSO_OBJECT_DigitalInput            =3200;
    public const int IPSO_OBJECT_DigitalOutput           =3201;
    public const int IPSO_OBJECT_AnalogueInput           =3202;
    public const int IPSO_OBJECT_AnalogueOutput          =3203;
    public const int IPSO_OBJECT_GenericSensor           =3300;
    public const int IPSO_OBJECT_IlluminanceSensor       =3301;
    public const int IPSO_OBJECT_PresenceSensor          =3302;
    public const int IPSO_OBJECT_TemperatureSensor       =3303;
    public const int IPSO_OBJECT_HumiditySensor          =3304;
    public const int IPSO_OBJECT_PowerMeasurement        =3305;
    public const int IPSO_OBJECT_Actuation               =3306;
    public const int IPSO_OBJECT_SetPoint                =3308;
    public const int IPSO_OBJECT_LoadControl             =3310;


    //LwM2M Resources
    //Reusable Resources
    public const int IPSO_RESOURCE_ObjectInstanceHandle   =4000;
    public const int IPSO_RESOURCE_ObjectVersion          =4001;
    public const int IPSO_RESOURCE_DigitalInputState      =5500;
    public const int IPSO_RESOURCE_DigitalInputCounter    =5501;
    public const int IPSO_RESOURCE_DigitalInputPolarity   =5502;
    public const int IPSO_RESOURCE_DigitalInputDebounce   =5503;
    public const int IPSO_RESOURCE_DigitalInputEdgeSelection  =5504;
    public const int IPSO_RESOURCE_DigitalInputCounterReset   =5505;
    public const int IPSO_RESOURCE_CurrentTime            =5506;
    public const int IPSO_RESOURCE_FractionalTime         =5507;
    
    public const int IPSO_RESOURCE_Latitude               =5514;
    public const int IPSO_RESOURCE_Longitude              =5515;
    public const int IPSO_RESOURCE_Uncertainty            =5516;
    public const int IPSO_RESOURCE_Velocity               =5517;
    public const int IPSO_RESOURCE_Timestamp              =5518;
    public const int IPSO_RESOURCE_MinLimit               =5519;
    public const int IPSO_RESOURCE_MaxLimit               =5520;
    public const int IPSO_RESOURCE_DelayDuration          =5521;
    public const int IPSO_RESOURCE_Trigger                =5523;
        
    public const int IPSO_RESOURCE_Duration               =5524;
    public const int IPSO_RESOURCE_MinimumOffTime         =5525;
    public const int IPSO_RESOURCE_Mode                   =5526;
    public const int IPSO_RESOURCE_DigitalOutputState     =5550;
    public const int IPSO_RESOURCE_DigitalOutputPolarity  =5551;
    public const int IPSO_RESOURCE_AnalogInputCurrentValue =5600;
    public const int IPSO_RESOURCE_MinMeasuredValue       =5601;
    public const int IPSO_RESOURCE_MaxMeasuredValue       =5602;
    public const int IPSO_RESOURCE_MinRangeValue          =5603;
    public const int IPSO_RESOURCE_MaxRangeValue          =5604;
    public const int IPSO_RESOURCE_AnalogOutputCurrentValue   =5650;
    public const int IPSO_RESOURCE_SensorValue            =5700;
    public const int IPSO_RESOURCE_SensorUnits            =5701;

    public const int IPSO_RESOURCE_APPLICATION_TYPE       =5750;
}

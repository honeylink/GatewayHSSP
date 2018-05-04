/**
 * @file ipso objects definition
 *
 */
#ifndef __IPSO_DEFINITION
#define __IPSO_DEFINITION

//Objects
//oma-lable Objects Produced by OMA
#define IPSO_OBJECT_LWM2M_Security          0
#define IPSO_OBJECT_LWM2M_Server            1
#define IPSO_OBJECT_AccessControl           2
#define IPSO_OBJECT_Device                  3
#define IPSO_OBJECT_Connectivity_Monitoring 4
#define IPSO_OBJECT_Firmware                5
#define IPSO_OBJECT_Location                6
#define IPSO_OBJECT_ConnectivityStatistics  7
#define IPSO_OBJECT_LockAndWipe             8
#define IPSO_OBJECT_SofwareManagement       9
#define IPSO_OBJECT_CellularConnectivity	10
#define IPSO_OBJECT_APNconnectionprofile    11
#define IPSO_OBJECT_WLANconnectivity        12
#define IPSO_OBJECT_Bearerselection         13
#define IPSO_OBJECT_SoftwareComponent       14
#define IPSO_OBJECT_DevCapMgmt              15
#define IPSO_OBJECT_Porfolio                16
#define IPSO_OBJECT_CommunicationsCharacteristics          17
#define IPSO_OBJECT_NASConfiguration        18
#define IPSO_OBJECT_LwM2MAPPDATA            19
#define IPSO_OBJECT_EventLog                20
#define IPSO_OBJECT_Pending                 21

//ext-label Objects Produced by 3rd party Standards Development Organizations
#define IPSO_OBJECT_CmdhPolicy              2048
#define IPSO_OBJECT_ActiveCmdhPolicy        2049
#define IPSO_OBJECT_CmdhDefaults            2050
#define IPSO_OBJECT_CmdhDefEcValues         2051
#define IPSO_OBJECT_CmdhEcDefParamValues    2052
#define IPSO_OBJECT_CmdhLimits              2053
#define IPSO_OBJECT_CmdhNetworkAccessRules  2054
#define IPSO_OBJECT_CmdhNwAccessRule        2055
#define IPSO_OBJECT_CmdhBuffer              2056
#define IPSO_OBJECT_CmdhBackOffParametersSet 2057
#define IPSO_OBJECT_DigitalInput            3200
#define IPSO_OBJECT_DigitalOutput           3201
#define IPSO_OBJECT_AnalogueInput           3202
#define IPSO_OBJECT_AnalogueOutput          3203
#define IPSO_OBJECT_GenericSensor           3300
#define IPSO_OBJECT_IlluminanceSensor       3301
#define IPSO_OBJECT_PresenceSensor          3302
#define IPSO_OBJECT_TemperatureSensor       3303
#define IPSO_OBJECT_HumiditySensor          3304
#define IPSO_OBJECT_PowerMeasurement        3305
#define IPSO_OBJECT_Actuation               3306
#define IPSO_OBJECT_SetPoint                3308
#define IPSO_OBJECT_LoadControl             3310



//LwM2M Resources
//Reusable Resources
#define IPSO_RESOURCE_ObjectInstanceHandle   4000
#define IPSO_RESOURCE_ObjectVersion          4001
#define IPSO_RESOURCE_DigitalInputState      5500
#define IPSO_RESOURCE_DigitalInputCounter    5501
#define IPSO_RESOURCE_DigitalInputPolarity   5502
#define IPSO_RESOURCE_DigitalInputDebounce   5503
#define IPSO_RESOURCE_DigitalInputEdgeSelection  5504
#define IPSO_RESOURCE_DigitalInputCounterReset   5505
#define IPSO_RESOURCE_CurrentTime            5506
#define IPSO_RESOURCE_FractionalTime         5507

#define IPSO_RESOURCE_Latitude               5514
#define IPSO_RESOURCE_Longitude              5515
#define IPSO_RESOURCE_Uncertainty            5516
#define IPSO_RESOURCE_Velocity               5517
#define IPSO_RESOURCE_Timestamp              5518
#define IPSO_RESOURCE_MinLimit               5519
#define IPSO_RESOURCE_MaxLimit               5520
#define IPSO_RESOURCE_DelayDuration          5521
#define IPSO_RESOURCE_Trigger                5523

#define IPSO_RESOURCE_Duration               5524
#define IPSO_RESOURCE_MinimumOffTime         5525
#define IPSO_RESOURCE_Mode                   5526
#define IPSO_RESOURCE_DigitalOutputState     5550
#define IPSO_RESOURCE_DigitalOutputPolarity  5551
#define IPSO_RESOURCE_AnalogInputCurrentValue 5600
#define IPSO_RESOURCE_MinMeasuredValue       5601
#define IPSO_RESOURCE_MaxMeasuredValue       5602
#define IPSO_RESOURCE_MinRangeValue          5603
#define IPSO_RESOURCE_MaxRangeValue          5604
#define IPSO_RESOURCE_AnalogOutputCurrentValue   5650
#define IPSO_RESOURCE_SensorValue            5700
#define IPSO_RESOURCE_SensorUnits            5701

#define IPSO_RESOURCE_APPLICATION_TYPE       5750


#endif

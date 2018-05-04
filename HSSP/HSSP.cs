using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LIBC;

namespace GatewayHSSP.HSSP
{
    using uint32_t = System.UInt32;
    using uint16_t = System.UInt16;
    using uint8_t = System.Byte;

    unsafe public struct HSSPPayload_t
    {
	    public  uint8_t ControlHeader;
	    public  uint8_t Option;
        public  fixed uint8_t DeviceID[15];
	    public  uint16_t Length;
	    public  uint8_t * ObjectData;
	    public  uint16_t CRC;
    };
    unsafe public struct HSSPObjectData_t
    {
	    public  uint16_t ObjectId;
	    public  uint8_t InstanceId;
	    public  uint16_t ResourceId;
	    public  uint8_t ValueType;
	    public  uint8_t valueLen;
	    public  uint8_t * pValue;
    };

    /* Parsed message struct */
    unsafe public struct HSSPPacket_t
    {
        public  uint8_t             *buffer; /* pointer to HSSP header / incoming packet buffer / memory to serialize packet */

        public  uint8_t             version;
        public  uint8_t            type;
        public  uint8_t             code;
        public  uint16_t            mid;
        public  uint16_t            payload_len;
        public  uint8_t            *payload;
        public  uint8_t             token_len;
    };

    unsafe public class HSSP
    {
        public const byte HSSP_HEADER_LEN               =          4; /* | version:0x03 type:0x0C tkl:0xF0 | code | mid:0x00FF | mid:0xFF00 | */
        public const byte HSSP_ETAG_LEN = 8;/* The maximum number of bytes for the ETag */
        public const byte HSSP_TOKEN_LEN = 8;/* The maximum number of bytes for the Token */
        public const byte HSSP_MAX_ACCEPT_NUM = 2; /* The maximum number of accept preferences to parse/store */
        public const byte HSSP_MAX_OPTION_HEADER_LEN = 5;

        public const byte HSSP_HEADER_VERSION_MASK = 0xC0;
        public const byte HSSP_HEADER_VERSION_POSITION = 6;
        public const byte HSSP_HEADER_TYPE_MASK = 0x30;
        public const byte HSSP_HEADER_TYPE_POSITION = 4;
        public const byte HSSP_HEADER_TOKEN_LEN_MASK = 0x0F;
        public const byte HSSP_HEADER_TOKEN_LEN_POSITION = 0;
        public const byte HSSP_HEADER_OPTION_DELTA_MASK = 0xF0;
        public const byte HSSP_HEADER_OPTION_SHORT_LENGTH_MASK = 0x0F;


        /**
         * @brief HSSP_pack
         */
        public static byte[] HSSP_packAck(byte code, uint16_t mid)
        {
            HSSPPacket_t HSSP_pkt;
            byte[] tempX = new byte[512];
            byte[] returnX = null; 

	        hssp_init_message((void*)&HSSP_pkt, MessageType.HSSP_TYPE_ACK, code, mid);

	        HSSP_pkt.payload_len = 0;
            HSSP_pkt.payload = null;
            fixed (byte* tmp = tempX)
            {
                int lengthOfHssp = hssp_serialize_message((void*)&HSSP_pkt, tmp);
                returnX = new byte[lengthOfHssp];
                Array.Copy(tempX, returnX, lengthOfHssp);
            }       

            return returnX;
        }

        /**
        * @brief HSSP_pack
        */
        static public byte[] HSSP_pack(HSSPPacket_t* HSSP_pkt, HSSPObjectData_t[] pObjectDataArray, string deviceId, uint8_t messagetype, uint8_t code, uint16_t mid)
        {
	        const uint16_t PacketLenMax = 512;

	        uint16_t ObjectDataLen = 0;

	        uint16_t PayloadLen = 0;

	        //1. construct the object data array
            byte[] ObjectDataBuffer = new byte[PacketLenMax];
	        foreach (var objectTmp in pObjectDataArray )
	        {
                //object id              
                Array.Copy(System.BitConverter.GetBytes(objectTmp.ObjectId), 0, ObjectDataBuffer, ObjectDataLen, 2);
                ObjectDataLen += 2;
                //instance id
                Array.Copy(System.BitConverter.GetBytes(objectTmp.InstanceId), 0, ObjectDataBuffer, ObjectDataLen, 1);
                ObjectDataLen += 1;
                //resource id
                Array.Copy(System.BitConverter.GetBytes(objectTmp.ResourceId), 0, ObjectDataBuffer, ObjectDataLen, 2);
                ObjectDataLen += 2;
                //value type
                Array.Copy(System.BitConverter.GetBytes(objectTmp.ValueType), 0, ObjectDataBuffer, ObjectDataLen, 1);
                ObjectDataLen += 1;
                //value len
                Array.Copy(System.BitConverter.GetBytes(objectTmp.valueLen), 0, ObjectDataBuffer, ObjectDataLen, 1);
                ObjectDataLen += 1;
                //value
                byte[] tmp = new byte[objectTmp.valueLen];
                for (uint32_t i = 0; i < objectTmp.valueLen; i++ )
                {
                    tmp[i] = objectTmp.pValue[i];
                }
                Array.Copy(tmp, 0, ObjectDataBuffer, ObjectDataLen, objectTmp.valueLen);
                ObjectDataLen += objectTmp.valueLen;
	        }

	        //2. construct the payload
            byte[] payloadBuffer = new byte[PacketLenMax];
            
            payloadBuffer[PayloadLen++] = 0;//ControlHeader
            
            payloadBuffer[PayloadLen++] = 0;//Option
            //device id
            byte[] deviceIdBuffer =  Enumerable.Repeat((byte)0,15).ToArray();
            Array.Copy(System.Text.Encoding.ASCII.GetBytes(deviceId), deviceIdBuffer, deviceId.Length);
            Array.Copy(deviceIdBuffer, 0, payloadBuffer, PayloadLen, deviceIdBuffer.Length);
            PayloadLen += (uint16_t)deviceIdBuffer.Length;
            //length
            Array.Copy(System.BitConverter.GetBytes(ObjectDataLen), 0, payloadBuffer, PayloadLen, 2);
            PayloadLen += 2;
            //object data
            Array.Copy(ObjectDataBuffer, 0, payloadBuffer, PayloadLen, ObjectDataLen);
            PayloadLen += ObjectDataLen;
            //crc
            payloadBuffer[PayloadLen++] = 0x00;//CRC
	        payloadBuffer[PayloadLen++] = 0x00;//CRC


	        //3. construct the packet
            uint32_t lengthOfHssp;
            byte[] packetBuffer = new byte[PacketLenMax];
            hssp_init_message(HSSP_pkt, (MessageType)messagetype, code, mid);
	        HSSP_pkt->payload_len = PayloadLen;
            fixed (byte* pPayloadBuffer = payloadBuffer)
            {
                HSSP_pkt->payload = pPayloadBuffer;
       
                fixed (byte* pPacketBuffer = packetBuffer)
                {
                    lengthOfHssp = (uint32_t)hssp_serialize_message(HSSP_pkt, pPacketBuffer);
                }                
            }

            byte[] returnX = new byte[lengthOfHssp];
            Array.Copy(packetBuffer, returnX, lengthOfHssp);            
            return returnX;
        }
         

        /**
         * @brief HLP_depack
         */
        void HSSP_depack(void* pSource, uint16_t len, uint8_t* p)
        {
            HSSPPacket_t HSSP_pkt;

            if (hssp_parse_message(&HSSP_pkt, p, len) != 0)
                return;

            if (HSSP_pkt.type == (uint8_t)MessageType.HSSP_TYPE_ACK)
            {
                //
            }

            if (HSSP_pkt.payload_len != 0)
            {
                //处理payload 业务
            }
        }

        /**
         * @brief hssp_init_message
         * @param packet caller allocate
         */
        static void hssp_init_message(void* packet,
                                MessageType type,
                                uint8_t code,
                                uint16_t mid)
        {
            HSSPPacket_t * HSSP_pkt = (HSSPPacket_t *)packet;

            /* Important thing */
            libc.memset(HSSP_pkt, 0x00, (uint)sizeof(HSSPPacket_t));            

            HSSP_pkt->type = (uint8_t)type;
            HSSP_pkt->code = code;
            HSSP_pkt->mid = mid;

            HSSP_pkt->token_len = 0;
        }

        /**
         * @brief hssp_serialize_message
         * @param packet caller allocate
         * @param buffer caller allocate to store all packet data
         */
        static int hssp_serialize_message(void* packet,
                                       uint8_t* buffer)
        {
              HSSPPacket_t * HSSP_pkt = (HSSPPacket_t *) packet;
              uint8_t *option;    

              /* Initialize */
              HSSP_pkt->buffer = buffer;
              HSSP_pkt->version = 0;

              /* set header fields */
              HSSP_pkt->buffer[0]  = 0x00;
              HSSP_pkt->buffer[0] |= (byte)(HSSP_HEADER_VERSION_MASK & (HSSP_pkt->version)<<HSSP_HEADER_VERSION_POSITION);
              HSSP_pkt->buffer[0] |= (byte)(HSSP_HEADER_TYPE_MASK & (HSSP_pkt->type)<<HSSP_HEADER_TYPE_POSITION);
              HSSP_pkt->buffer[0] |= (byte)(HSSP_HEADER_TOKEN_LEN_MASK & (HSSP_pkt->token_len) << HSSP_HEADER_TOKEN_LEN_POSITION);
              HSSP_pkt->buffer[1] = HSSP_pkt->code;
              HSSP_pkt->buffer[2] = (uint8_t) ((HSSP_pkt->mid)>>8);
              HSSP_pkt->buffer[3] = (uint8_t) (HSSP_pkt->mid);


              option = HSSP_pkt->buffer + HSSP_HEADER_LEN;

              /* Pack payload */
              /* Payload marker */
              if (HSSP_pkt->payload_len > 0)
              {
                *option = 0xFF;
                ++option;
                libc.memcpy(option, HSSP_pkt->payload, HSSP_pkt->payload_len);
              }
              return (int)(option - buffer) + HSSP_pkt->payload_len; /* packet length */
        }

        /**
         * @brief hssp_parse_message
         * @param request : caller allocate to store packet information
         * @param data : all packet buffer
         */
        static public int hssp_parse_message(void* request,
                                          uint8_t* data,
                                          uint16_t data_len)
        {
          uint8_t *current_option;
          HSSPPacket_t * HSSP_pkt = (HSSPPacket_t *)request;

          /* Initialize packet */
          libc.memset(HSSP_pkt, 0x00, (uint)sizeof(HSSPPacket_t));

          /* pointer to packet bytes */
          HSSP_pkt->buffer = data;

          /* parse header fields */
          HSSP_pkt->version = (byte)((HSSP_HEADER_VERSION_MASK & HSSP_pkt->buffer[0]) >> HSSP_HEADER_VERSION_POSITION);
          HSSP_pkt->type = (byte)((HSSP_HEADER_TYPE_MASK & HSSP_pkt->buffer[0])>>HSSP_HEADER_TYPE_POSITION);
          HSSP_pkt->token_len = (byte)(libc.MIN(HSSP_TOKEN_LEN, (HSSP_HEADER_TOKEN_LEN_MASK & HSSP_pkt->buffer[0]) >> HSSP_HEADER_TOKEN_LEN_POSITION));
          HSSP_pkt->code = HSSP_pkt->buffer[1];
          HSSP_pkt->mid = (uint16_t)(HSSP_pkt->buffer[2]<<8 | HSSP_pkt->buffer[3]);

          if (HSSP_pkt->version != 0)
          {
            return -1;
          }

          current_option = data + HSSP_HEADER_LEN;

          /* parse options */
          current_option += HSSP_pkt->token_len;

          if (current_option - data < data_len)
          {
              /* Payload marker 0xFF, currently only checking for 0xF* because rest is reserved */
              if ((current_option[0] & 0xF0) == 0xF0)
              {
                  HSSP_pkt->payload = ++current_option;
                  HSSP_pkt->payload_len = (uint16_t)(data_len - (HSSP_pkt->payload - data));

                  return 0;
              }
          }

          return 0;
        }

    }

    /* message types */
    public enum MessageType
    {
        HSSP_TYPE_CON = 0, /* confirmables */
        HSSP_TYPE_NON, /* non-confirmables */
        HSSP_TYPE_ACK, /* acknowledgements */
        HSSP_TYPE_RST  /* reset */
    } ;

    /* request method codes */
    public enum MethodCodes
    {
        HSSP_GET = 1,
        HSSP_POST,
        HSSP_PUT,
        HSSP_DELETE
    };

    /* response codes */
    public enum ResponseCode
    {
        NO_ERROR                      = 0,

        CREATED_2_01                  = 65,  /* CREATED */
        DELETED_2_02                  = 66,  /* DELETED */
        VALID_2_03                    = 67,  /* NOT_MODIFIED */
        CHANGED_2_04                  = 68,  /* CHANGED */
        CONTENT_2_05                  = 69,  /* OK */
                                         
        BAD_REQUEST_4_00              = 128, /* BAD_REQUEST */
        UNAUTHORIZED_4_01             = 129, /* UNAUTHORIZED */
        BAD_OPTION_4_02               = 130, /* BAD_OPTION */
        FORBIDDEN_4_03                = 131, /* FORBIDDEN */
        NOT_FOUND_4_04                = 132, /* NOT_FOUND */
        METHOD_NOT_ALLOWED_4_05       = 133, /* METHOD_NOT_ALLOWED */
        NOT_ACCEPTABLE_4_06           = 134, /* NOT_ACCEPTABLE */
        PRECONDITION_FAILED_4_12      = 140, /* BAD_REQUEST */
        REQUEST_ENTITY_TOO_LARGE_4_13 = 141, /* REQUEST_ENTITY_TOO_LARGE */
        UNSUPPORTED_MEDIA_TYPE_4_15   = 143, /* UNSUPPORTED_MEDIA_TYPE */
                                         
        INTERNAL_SERVER_ERROR_5_00    = 160, /* INTERNAL_SERVER_ERROR */
        NOT_IMPLEMENTED_5_01          = 161, /* NOT_IMPLEMENTED */
        BAD_GATEWAY_5_02              = 162, /* BAD_GATEWAY */
        SERVICE_UNAVAILABLE_5_03      = 163, /* SERVICE_UNAVAILABLE */
        GATEWAY_TIMEOUT_5_04          = 164, /* GATEWAY_TIMEOUT */
        PROXYING_NOT_SUPPORTED_5_05   = 165, /* PROXYING_NOT_SUPPORTED */

        /* Erbium errors */
        MEMORY_ALLOCATION_ERROR         = 192,
        PACKET_SERIALIZATION_ERROR,

        /* Erbium hooks */
        MANUAL_RESPONSE
    } ;


    public enum HSSPValueType
    {
        VALUE_UNKNOWN =     0x0,
        VALUE_BOOLEAN =       0x1,
        VALUE_INTEGER =       0x2,
        VALUE_FLOAT =         0x3,
        VALUE_STRING =        0x4,
        VALUE_BINARY =        0x5,
    }
}

using Konbini.RfidFridge.TagManagement.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Konbini.RfidFridge.TagManagement.Service
{
    public class RfidReaderInterface : IRfidReaderInterface
    {
        public UIntPtr hreader;
        Thread InvenThread;
        private bool _shouldStop;
        Byte[] AntennaSel = new byte[16];
        Byte AntennaSelCount = 0;
        public Byte enableAFI;
        public Byte AFI;

        public RfidReaderInterface()
        {
            // Load DLL
            RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");
            Tags = new List<string>();
            OldTags = new List<string>();
            TagRawData = new List<Tuple<string, int>>();
            OldHashTags = string.Empty;
            HashTags = string.Empty;
        }

        public Action OnRecordFinish { get; set; }
        public Action<List<string>> OnTagsRecord { get; set; }

        public List<string> Tags { get; set; }
        private List<string> OldTags { get; set; }

        public string HashTags { get; set; }
        public string OldHashTags { get; set; }
        public int RecordCount { get; set; }
        public List<Tuple<string, int>> TagRawData { get; set; }
        public bool Connect()
        {
            var connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + "M201" + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_USB + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + "0" + ";" +
             RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + "";

            //var connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + "RD5100" + ";" +
            //   RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_COM + ";" +
            //   RFIDLIB.rfidlib_def.CONNSTR_NAME_COMNAME + "=" + "COM2" + ";" +
            //   RFIDLIB.rfidlib_def.CONNSTR_NAME_COMBARUD + "=" + "38400" + ";" +
            //   RFIDLIB.rfidlib_def.CONNSTR_NAME_COMFRAME + "=" + "8E1" + ";" +
            //   RFIDLIB.rfidlib_def.CONNSTR_NAME_BUSADDR + "=" + "255";

            var iret = RFIDLIB.rfidlib_reader.RDR_Open(connstr, ref hreader);
            SeriLogService.LogInfo("Connect to hardware: " + iret);
            return iret == 0;
        }

        public void StartRecord()
        {
            RecordCount = 0;
            InvenThread = new Thread(DoInventory);
            InvenThread.Start();
        }

        public void StopRecord()
        {
            _shouldStop = true;
            RFIDLIB.rfidlib_reader.RDR_SetCommuImmeTimeout(hreader);
        }

        public List<string> GetTags()
        {
            Console.WriteLine(Tags.Count);

            return Tags;
        }

        public void DoInventory()
        {
            int iret;
            Byte AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;

            while (!_shouldStop)
            {

                UInt32 nTagCount = 0;
                iret = TagInventory(AIType, AntennaSelCount, AntennaSel, ref nTagCount);
                if (iret == 0)
                {
                    // inventory ok
                }
                else
                {
                    // inventory error 
                }
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            }
            OnRecordFinish?.Invoke();
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(hreader);

        }
        public int TagInventory(Byte AIType, Byte AntennaSelCount, Byte[] AntennaSel, ref UInt32 nTagCount)
        {

            int iret;
            UIntPtr InvenParamSpecList = UIntPtr.Zero;
            InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
            if (InvenParamSpecList.ToUInt64() != 0)
            {
                RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
            }
            nTagCount = 0;
            LABEL_TAG_INVENTORY:
            iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel, InvenParamSpecList);
            if (iret == 0 || iret == -21)
            {
                var tags = new List<string>();
                nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                UIntPtr TagDataReport;
                TagDataReport = (UIntPtr)0;
                TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                OldHashTags = HashTags;
                OldTags = Tags;
                while (TagDataReport.ToUInt64() > 0)
                {
                    UInt32 aip_id = 0;
                    UInt32 tag_id = 0;
                    UInt32 ant_id = 0;
                    Byte dsfid = 0;
                    Byte uidlen = 0;
                    Byte[] uid = new Byte[16];

                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                    if (iret == 0)
                    {
                        uidlen = 8;
                        var tagId = BitConverter.ToString(uid, 0, (int)uidlen).Replace("-", string.Empty);
                        tags.Add(tagId);
                    }
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                }

                Tags = tags;
                var orderedTags = tags.OrderBy(x => x);
                HashTags = string.Join("|", orderedTags);
                if (OldHashTags != HashTags)
                {
                    OnTagsRecord?.Invoke(Tags);
                    if (OldHashTags.Length > HashTags.Length)
                    {

                        var removed = OldTags.Except(Tags);
                        Console.WriteLine("Removed: " + string.Join("|", removed));
                    }
                    if (OldHashTags.Length < HashTags.Length)
                    {
                        var added = Tags.Except(OldTags);
                        Console.WriteLine("Added: " + string.Join("|", added));
                    }
                    if (OldHashTags.Length == HashTags.Length)
                    {
                    }
                }
                if (iret == -21)
                {
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;//use only-new-tag inventory 
                    goto LABEL_TAG_INVENTORY;
                }
                iret = 0;
            }
            if (InvenParamSpecList.ToUInt64() != 0) RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
            return iret;
        }
    }
}

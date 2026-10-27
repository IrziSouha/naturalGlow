using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class FacturaProveidor
    {
        public FacturaProveidor()
        {
            Id = "-";
            IdProveidor = "-";
            Data = DateTime.Now;
            LlistaProductes = new List<LiniaFacturaProveidor>();
        }

        public FacturaProveidor(
            string id,
            string idProveidor,
            DateTime data,
            List<LiniaFacturaProveidor> llistaProductes)
        {
            Id = id;
            IdProveidor = idProveidor;
            Data = data;
            LlistaProductes = llistaProductes;
        }

        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string IdProveidor { get; set; }

        [FirestoreProperty]
        public DateTime Data { get; set; }

        [FirestoreProperty]
        public List<LiniaFacturaProveidor> LlistaProductes { get; set; }

        public override string ToString()
        {
            return $"{Id} - {IdProveidor} - {Data:dd/MM/yyyy}";
        }
    }
}
// using System;
// using System.Diagnostics;
// using System.Threading;
//
// namespace msgqNET.implementations.msgq;
//
// public static class Msgq
// {
//     public const int NUM_READERS = 32; // Assuming a constant value
//
//     public static void MsgqInitSubscriber(ref MsgqQueue q)
//     {
//         Debug.Assert(q != null);
//         Debug.Assert(q.NumReaders != null);
//
//         ulong uid = MsgqGetUid();
//
//         while (true)
//         {
//             ulong curNumReaders = q.NumReaders.Value;
//             ulong newNumReaders = curNumReaders + 1;
//
//             // No more slots available. Reset all subscribers to kick out inactive ones
//             if (newNumReaders > NUM_READERS)
//             {
//                 Console.WriteLine("Warning, evicting all subscribers!");
//                 q.NumReaders.Value = 0;
//
//                 for (int i = 0; i < NUM_READERS; i++)
//                 {
//                     q.ReadValids[i].Value = false;
//
//                     ulong oldUid = q.ReadUids[i].Value;
//                     q.ReadUids[i].Value = 0;
//
//                     // Wake up reader in case they are in a poll
//                     ThreadSignal((int)(oldUid & 0xFFFFFFFF));
//                 }
//
//                 continue;
//             }
//
//             // Use atomic compare and exchange to handle race condition where two subscribers start at the same time
//             if (Interlocked.CompareExchange(ref q.NumReaders.Value, newNumReaders, curNumReaders) == curNumReaders)
//             {
//                 q.ReaderId = curNumReaders;
//                 q.ReadUidLocal = uid;
//
//                 q.ReadValids[curNumReaders].Value = false;
//                 q.ReadPointers[curNumReaders].Value = 0;
//                 q.ReadUids[curNumReaders].Value = uid;
//                 break;
//             }
//         }
//
//         Console.WriteLine($"New subscriber id: {q.ReaderId} uid: {q.ReadUidLocal} {q.Endpoint}");
//         MsgqResetReader(ref q);
//     }
//
//     // Assuming implementation of GetUid, ThreadSignal, ResetReader and supporting classes
//     private static ulong MsgqGetUid()
//     {
//         // Example UUID generation
//         return (ulong)DateTime.Now.Ticks;
//     }
//
//     private static void ThreadSignal(int uid)
//     {
//         // Example signal to a specific thread (Placeholder implementation)
//         Console.WriteLine($"ThreadSignal called for uid: {uid}");
//     }
//
//     private static void MsgqResetReader(ref MsgqQueue q)
//     {
//         // Reset reader logic (Placeholder implementation)
//         Console.WriteLine($"Resetting reader for {q.Endpoint}");
//     }
// }
//
// public class MsgqQueue
// {
//     public AtomicUlong NumReaders { get; set; } = new AtomicUlong();
//     public ulong ReaderId { get; set; }
//     public ulong ReadUidLocal { get; set; }
//     public AtomicBool[] ReadValids { get; set; } = new AtomicBool[Msgq.NUM_READERS];
//     public AtomicUlong[] ReadPointers { get; set; } = new AtomicUlong[Msgq.NUM_READERS];
//     public AtomicUlong[] ReadUids { get; set; } = new AtomicUlong[Msgq.NUM_READERS];
//     public string Endpoint { get; set; }
//
//     public MsgqQueue()
//     {
//         for (int i = 0; i < Msgq.NUM_READERS; i++)
//         {
//             ReadValids[i] = new AtomicBool();
//             ReadPointers[i] = new AtomicUlong();
//             ReadUids[i] = new AtomicUlong();
//         }
//     }
// }
//
// public class AtomicBool
// {
//     private int value;
//
//     public bool Value
//     {
//         get => value == 1;
//         set => Interlocked.Exchange(ref this.value, value ? 1 : 0);
//     }
// }
//
// public class AtomicUlong
// {
//     private long value;
//
//     public ulong Value
//     {
//         get => (ulong)Interlocked.Read(ref value);
//         set => Interlocked.Exchange(ref this.value, (long)value);
//     }
// }
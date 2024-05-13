﻿namespace AutoRetainer.Scheduler.Tasks;

internal static class TaskPostprocessRetainerIPC
{
		internal static void Enqueue(string retainer, string pluginToProcess = null)
		{
				P.TaskManager.Enqueue(() =>
				{
						SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Clear();
						IPC.FireRetainerPostprocessTaskRequestEvent(retainer);
				}, "TaskRetainerPostprocessIPCEnqueue");
				P.TaskManager.Enqueue(() =>
				{
						DebugLog($"SchedulerMain.RetainerPostprocess contains: {SchedulerMain.RetainerPostprocess.Print()}, pluginToProcess = {pluginToProcess}");
						foreach (var x in SchedulerMain.RetainerPostprocess.Where(x => pluginToProcess == null || x == pluginToProcess))
						{
								P.TaskManager.EnqueueImmediate(() =>
								{
										SchedulerMain.RetainerPostprocess = SchedulerMain.RetainerPostprocess.Remove(x);
										SchedulerMain.RetainerPostProcessLocked = true;
										IPC.FireRetainerPostprocessEvent(x, retainer);
								}, $"Retainer Postprocess request from {x}");
								P.TaskManager.EnqueueImmediate(() => !SchedulerMain.RetainerPostProcessLocked, int.MaxValue, $"Retainer Postprocess task from {x}");
						}
				}, "TaskRetainerPostprocessProcessEntries");
		}
}

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Gates;
public static class DomainRunner
{
 public static int Main(string[] args)
 {
  int passed=0, failed=0;
  var assembly=Assembly.LoadFrom(args[0]);
  foreach(var name in new[]{"SeaLion.Tests.EditMode.Combat.OrdinaryCombatSystemTests","SeaLion.Tests.EditMode.Combat.HarborGuardianControllerTests","SeaLion.Tests.EditMode.Gates.GateResolverTests","SeaLion.Tests.EditMode.Crowd.ForceRuntimeTests","SeaLion.Tests.EditMode.Battle.BattleLifecycleTests","SeaLion.Tests.EditMode.Battle.BattleResultControllerTests"})
  {
   var type=assembly.GetType(name,true);
   foreach(var method in type.GetMethods().Where(m=>m.GetCustomAttributes(typeof(TestAttribute),false).Length>0 && m.GetParameters().Length==0))
   {
    try { method.Invoke(Activator.CreateInstance(type),null); passed++; Console.WriteLine("PASS "+type.Name+"."+method.Name); }
    catch(Exception ex) { failed++; Console.WriteLine("FAIL "+method.Name+": "+(ex.InnerException??ex)); }
   }
  }
  try {
   var resolver=new GateResolver(300); var gate=new StableId("overflow-gate"); var member=new StableId("craft-1");
   try { resolver.Resolve(gate,GateOutcome.Multiply,4f,default(StableId),int.MaxValue,member); throw new Exception("overflow not rejected"); } catch(OverflowException) {}
   if(!resolver.Resolve(gate,GateOutcome.Multiply,4f,default(StableId),8,member).Applied) throw new Exception("failed arithmetic consumed key");
   passed++; Console.WriteLine("PASS ArithmeticFailureDoesNotConsumeGateKey");
  } catch(Exception ex) { failed++; Console.WriteLine("FAIL "+ex); }
  Console.WriteLine("Domain tests: passed="+passed+" failed="+failed); return failed==0?0:1;
 }
}

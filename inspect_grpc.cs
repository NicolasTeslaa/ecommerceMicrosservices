using System;
using System.Linq;
using System.Reflection;
using Grpc.Core;

Console.WriteLine("AuthContext ctors:");
foreach (var c in typeof(AuthContext).GetConstructors(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
    Console.WriteLine(c);

Console.WriteLine("ServerCallContext abstract members:");
var members = typeof(ServerCallContext).GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public)
    .Where(m => m.IsAbstract)
    .OrderBy(m => m.Name);
foreach (var m in members)
    Console.WriteLine(m);

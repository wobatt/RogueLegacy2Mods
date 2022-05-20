using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Wob_Common {
    // Class to hold sets of OpCodes for use in transpiler tests
    internal static class OpCodeSet {
        // Most of these are to combine int32 and int8 variants, as what dnSpy/ILSpy display and what the transpiler detects are not always the same
        public static readonly OpCode[] Beq     = { OpCodes.Beq, OpCodes.Beq_S };
        public static readonly OpCode[] Bge     = { OpCodes.Bge, OpCodes.Bge_S };
        public static readonly OpCode[] Bge_Un  = { OpCodes.Bge_Un, OpCodes.Bge_Un_S };
        public static readonly OpCode[] Bgt     = { OpCodes.Bgt, OpCodes.Bgt_S };
        public static readonly OpCode[] Bgt_Un  = { OpCodes.Bgt_Un, OpCodes.Bgt_Un_S };
        public static readonly OpCode[] Ble     = { OpCodes.Ble, OpCodes.Ble_S };
        public static readonly OpCode[] Ble_Un  = { OpCodes.Ble_Un, OpCodes.Ble_Un_S };
        public static readonly OpCode[] Blt     = { OpCodes.Blt, OpCodes.Blt_S };
        public static readonly OpCode[] Blt_Un  = { OpCodes.Blt_Un, OpCodes.Blt_Un_S };
        public static readonly OpCode[] Bne_Un  = { OpCodes.Bne_Un, OpCodes.Bne_Un_S };
        public static readonly OpCode[] Br      = { OpCodes.Br, OpCodes.Br_S };
        public static readonly OpCode[] Brfalse = { OpCodes.Brfalse, OpCodes.Brfalse_S };
        public static readonly OpCode[] Brtrue  = { OpCodes.Brtrue, OpCodes.Brtrue_S };
        public static readonly OpCode[] Ldarg   = { OpCodes.Ldarg, OpCodes.Ldarg_S, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };
        public static readonly OpCode[] Ldarga  = { OpCodes.Ldarga, OpCodes.Ldarga_S };
        public static readonly OpCode[] Ldc_I4  = { OpCodes.Ldc_I4, OpCodes.Ldc_I4_S, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, OpCodes.Ldc_I4_M1 };
        public static readonly OpCode[] Ldloc   = { OpCodes.Ldloc, OpCodes.Ldloc_S, OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3 };
        public static readonly OpCode[] Ldloca  = { OpCodes.Ldloca, OpCodes.Ldloca_S };
        public static readonly OpCode[] Leave   = { OpCodes.Leave, OpCodes.Leave_S };
        public static readonly OpCode[] Starg   = { OpCodes.Starg, OpCodes.Starg_S };
        public static readonly OpCode[] Stloc   = { OpCodes.Stloc, OpCodes.Stloc_S, OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3 };
        // Boolean literals are 0 (false) or 1 (true), so this is for them 
        public static readonly OpCode[] Ldc_I4_Bool = { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1 };
        // These are grouped by operand type, mostly used in my transpiler class
        public static readonly OpCode[] OperandType_FieldInfo       = { OpCodes.Ldfld, OpCodes.Ldsfld, OpCodes.Stfld, OpCodes.Stsfld };
        public static readonly OpCode[] OperandType_MethodInfo      = { OpCodes.Call, OpCodes.Callvirt, OpCodes.Ldftn, OpCodes.Ldvirtftn };
        public static readonly OpCode[] OperandType_ConstructorInfo = { OpCodes.Newobj };
        public static readonly OpCode[] OperandType_Type            = { OpCodes.Isinst };
    }

    // Class for making transpilers easier and more readable
    internal class WobTranspiler {
        // Field holding the instructions to be patched
        public List<CodeInstruction> CodeList { get; private set; }
        // Field showing how many times the block has been matched and actions implemented
        public int NumPatched { get; private set; }

        public WobTranspiler( IEnumerable<CodeInstruction> instructions ) {
            this.CodeList = new List<CodeInstruction>( instructions );
            this.NumPatched = 0;
        }

        // Get the results of the applied patches
        public IEnumerable<CodeInstruction> GetResult() {
            return this.CodeList.AsEnumerable();
        }

        // Apply the patch actions for each occurrence of the matched lines
        public void PatchAll( List<OpTest> tests, List<OpAction> actions ) { this.Patch( tests, actions, true ); }

        // Apply the patch actions for the first occurrence of the matched lines
        public void PatchFirst( List<OpTest> tests, List<OpAction> actions ) { this.Patch( tests, actions, false, 1 ); }

        // Apply the patch actions for the nth occurrence of the matched lines
        public void PatchOnce( List<OpTest> tests, List<OpAction> actions, int occurrence ) { this.Patch( tests, actions, false, occurrence ); }

        // Apply the patch actions
        public void Patch( List<OpTest> tests, List<OpAction> actions, bool patchAll = false, int occurrence = 1 ) {
            int patchCount = 0;
            WobPlugin.Log( "Starting search for instructions" );
            // Start at the beginning of the method
            int index = -1;
            // Keep track of how many occurrences have been found so far
            int occurrenceCount = 0;
            // Find the next occurrence
            while( this.GetNext( this.CodeList, tests, ref index ) ) {
                // Increment the counter, then check id it matches the parameter
                occurrenceCount++;
                if( patchAll || ( occurrenceCount == occurrence ) ) {
                    WobPlugin.Log( "Occurrence found at " + index + " - patching" );
                    // Perform each of the patching actions in turn
                    foreach( OpAction action in actions ) {
                        action.Do( this.CodeList, index );
                    }
                    // Record that an occurrence has been patched
                    patchCount++;
                    this.NumPatched++;
                }
            }
            WobPlugin.Log( patchCount + " occurrence" + ( patchCount == 1 ? "" : "s" ) + " patched" );
        }

        // Find the next instance of the block in the instructions, if it exists, and return the index where it is found
        private bool GetNext( List<CodeInstruction> codes, List<OpTest> tests, ref int index ) {
            // Make sure we don't go out of bounds, and we don't loop infinitely
            index = index < 0 ? 0 : ( index + 1 );
            // Go through the instruction list
            for( int i = index; i < ( codes.Count - tests.Count ); i++ ) {
                // Check if block matches the current index in the instruction list
                if( this.Test( codes, tests, i ) ) {
                    // Found, so set the index and return true
                    index = i;
                    return true;
                }
            }
            // Not found, so set an invalid index
            index = -1;
            return false;
        }

        // Check whether the tests in this block match the instruction list at the specified starting index
        private bool Test( List<CodeInstruction> codes, List<OpTest> tests, int startAt ) {
            bool found = true;
            // Make sure we don't go out of bounds
            if( 0 <= startAt && startAt < ( codes.Count - tests.Count ) ) {
                // Go through the list of tests
                for( int i = 0; i < tests.Count; i++ ) {
                    // Check if the current instruction matches the test
                    if( !tests[i].IsOp( codes[startAt + i] ) ) {
                        // If not a match, immediately exit and return no match
                        found = false;
                        break;
                    }
                }
            } else {
                // Out of bounds - clearly not a match
                found = false;
            }
            return found;
        }

        // Class for the instructions to be found and the actions to be performed to patch them
        public class OpTestActionPair {
            public List<OpTest> Tests { get; private set; }
            public List<OpAction> Actions { get; private set; }
            public OpTestActionPair( List<OpTest> tests, List<OpAction> actions ) {
                this.Tests = tests;
                this.Actions = actions;
            }
        }

        // Class for a test against a single instruction
        public class OpTest {
            private readonly OpCode[] opcodes;
            private readonly object operand;
            private readonly Type type;
            private readonly string name;

            // Constructors taking all conditions that the instruction must satisfy to pass the test - params are null for no checks
            public OpTest( OpCode opcode, object operand = null, Type type = null, string name = null ) : this( new OpCode[] { opcode }, operand, type, name ) { }
            public OpTest( OpCode[] opcodes, object operand = null, Type type = null, string name = null ) {
                this.opcodes = opcodes;
                this.operand = operand;
                this.type = type;
                this.name = name;
            }

            // Check an instruction against the conditions - skipping any nulls
            public bool IsOp( CodeInstruction code ) {
                return ( this.opcodes == null || this.opcodes.Contains( code.opcode ) ) && ( this.operand == null || this.HasOperand( code ) ) && ( this.name == null || this.HasName( code ) ) && ( this.type == null || this.HasType( code ) );
            }

            // Compare operands - needs to be instruction OpCode dependant as these define the type of the operand
            private bool HasOperand( CodeInstruction code ) {
                // 32-bit integer
                if( code.opcode == OpCodes.Ldc_I4 ) {                                 return ( (int)code.operand )             == ( (int)this.operand );             }
                // 8-bit integer
                if( code.opcode == OpCodes.Ldc_I4_S ) {                               return ( (sbyte)code.operand )           == ( (sbyte)this.operand );           }
                // 64-bit integer
                if( code.opcode == OpCodes.Ldc_I8 ) {                                 return ( (long)code.operand )            == ( (long)this.operand );            }
                // 32-bit float
                if( code.opcode == OpCodes.Ldc_R4 ) {                                 return ( (float)code.operand )           == ( (float)this.operand );           }
                // 64-bit float
                if( code.opcode == OpCodes.Ldc_R8 ) {                                 return ( (double)code.operand )          == ( (double)this.operand );          }
                // String
                if( code.opcode == OpCodes.Ldstr ) {                                  return ( (string)code.operand )          == ( (string)this.operand );          }
                // Fields
                if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode ) ) {       return ( (FieldInfo)code.operand )       == ( (FieldInfo)this.operand );       }
                // Methods
                if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode ) ) {      return ( (MethodInfo)code.operand )      == ( (MethodInfo)this.operand );      }
                // Constructors
                if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) ) { return ( (ConstructorInfo)code.operand ) == ( (ConstructorInfo)this.operand ); }
                // Types
                if( OpCodeSet.OperandType_Type.Contains( code.opcode ) ) {            return ( (Type)code.operand )            == ( (Type)this.operand );            }
                // No implemented yet - log it then return no match
                WobPlugin.Log( "ERROR: No operand comparison implemented for " + code.opcode, WobPlugin.ERROR );
                return false;
            }

            // Compare the declaring type on instructions
            private bool HasType( CodeInstruction code ) {
                // Fields
                if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode ) ) {       return ( (FieldInfo)code.operand ).DeclaringType == this.type;       }
                // Methods
                if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode ) ) {      return ( (MethodInfo)code.operand ).DeclaringType == this.type;      }
                // Constructors
                if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) ) { return ( (ConstructorInfo)code.operand ).DeclaringType == this.type; }
                // Types
                if( OpCodeSet.OperandType_Type.Contains( code.opcode ) ) {            return ( (Type)code.operand ) == this.type;                          }
                // No implemented yet - log it then return no match
                WobPlugin.Log( "ERROR: No type comparison implemented for " + code.opcode, WobPlugin.ERROR );
                return false;
            }

            // Compare the name of fields and methods on instructions
            private bool HasName( CodeInstruction code ) {
                // Fields
                if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode ) ) {       return ( (FieldInfo)code.operand ).Name == this.name;       }
                // Methods
                if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode ) ) {      return ( (MethodInfo)code.operand ).Name == this.name;      }
                // Constructors
                if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) ) { return ( (ConstructorInfo)code.operand ).Name == this.name; }
                // No implemented yet - log it then return no match
                WobPlugin.Log( "ERROR: No name comparison implemented for " + code.opcode, WobPlugin.ERROR );
                return false;
            }
        }

        public abstract class OpAction {
            // Index relative to the start of the matched code block for where to apply this action
            protected readonly int relativeIndex;

            // Constructor that just sets the relative index
            protected OpAction( int relativeIndex ) { this.relativeIndex = relativeIndex; }

            // Method that applies the action to the code list starting at the specified offset
            public abstract void Do( List<CodeInstruction> codes, int startAt );

            // Overwrite the opcode for an instruction at a specific index in the list
            protected void SetOp( List<CodeInstruction> codes, int index, OpCode opcode ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index].opcode = opcode;
                }
            }
            // Overwrite the opcode and operand for an instruction at a specific index in the list
            protected void SetOp( List<CodeInstruction> codes, int index, OpCode opcode, object operand ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index].opcode = opcode;
                    codes[index].operand = operand;
                }
            }
            // Overwrite the instruction at a specific index in the list
            protected void SetOp( List<CodeInstruction> codes, int index, CodeInstruction instruction ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index] = instruction;
                }
            }

            // Overwrite the opcode and operand with a non-functional operation for an instruction at a specific index in the list
            protected void RemoveOp( List<CodeInstruction> codes, int index ) { this.SetOp( codes, index, OpCodes.Nop, null ); }

            // Overwite all instructions in a range with non-functional operations
            protected void RemoveOps( List<CodeInstruction> codes, int startAt, int count ) {
                if( 0 <= startAt && startAt < ( codes.Count - count ) ) {
                    for( int i = 0; i < count; i++ ) {
                        this.RemoveOp( codes, startAt + i );
                    }
                }
            }

            // Insert a new instruction at the specified index
            protected void InsertOp( List<CodeInstruction> codes, int index, OpCode opcode, object operand = null ) { this.InsertOp( codes, index, new CodeInstruction( opcode, operand ) ); }
            protected void InsertOp( List<CodeInstruction> codes, int index, CodeInstruction instruction ) {
                if( 0 <= index && index < codes.Count ) {
                    codes.Insert( index, instruction );
                }
            }

            // Insert a set of new instructions at the specified index
            protected void InsertOps( List<CodeInstruction> codes, int index, List<CodeInstruction> instructions ) {
                if( 0 <= index && index < codes.Count ) {
                    codes.InsertRange( index, instructions );
                }
            }
        }

        // Overwrite the opcode on an instruction
        public class OpAction_SetOpcode : OpAction {
            private readonly OpCode opcode;
            public OpAction_SetOpcode( int relativeIndex, OpCode opcode ) : base( relativeIndex ) { this.opcode = opcode; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                this.SetOp( codes, startAt + this.relativeIndex, this.opcode );
            }
        }

        // Overwrite the operand on an instruction
        public class OpAction_SetOperand : OpAction {
            private readonly object operand;
            public OpAction_SetOperand( int relativeIndex, object operand ) : base( relativeIndex ) { this.operand = operand; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                int index = startAt + this.relativeIndex;
                this.SetOp( codes, index, codes[index].opcode, this.operand );
            }
        }

        // Overwrite the opcode and operand on an instruction
        public class OpAction_SetOperation : OpAction {
            private readonly OpCode opcode;
            private readonly object operand;
            public OpAction_SetOperation( int relativeIndex, OpCode opcode, object operand ) : base( relativeIndex ) { this.opcode = opcode; this.operand = operand; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                this.SetOp( codes, startAt + this.relativeIndex, this.opcode, this.operand );
            }
        }

        // Overwrite an entire instruction - be careful with this as the instruction may have a label on it
        public class OpAction_SetInstruction : OpAction {
            private readonly CodeInstruction instruction;
            private readonly bool safe;
            public OpAction_SetInstruction( int relativeIndex, CodeInstruction instruction, bool safe = true ) : base( relativeIndex ) { this.instruction = instruction; this.safe = safe; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                if( safe ) {
                    this.SetOp( codes, startAt + this.relativeIndex, this.instruction.opcode, this.instruction.operand );
                } else {
                    this.SetOp( codes, startAt + this.relativeIndex, this.instruction );
                }
            }
        }

        // Overwrite a series of instructions - be careful with this as the instructions may have labels on them
        public class OpAction_SetInstructions : OpAction {
            private readonly List<CodeInstruction> instructions;
            private readonly bool safe;
            public OpAction_SetInstructions( int relativeIndex, List<CodeInstruction> instructions, bool safe = true ) : base( relativeIndex ) { this.instructions = instructions; this.safe = safe; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                for( int i = 0; i < this.instructions.Count; i++ ) {
                    if( safe ) {
                        this.SetOp( codes, startAt + this.relativeIndex + i, this.instructions[i].opcode, this.instructions[i].operand );
                    } else {
                        this.SetOp( codes, startAt + this.relativeIndex + i, this.instructions[i] );
                    }
                }
            }
        }

        // Insert a set of new instructions
        public class OpAction_Insert : OpAction {
            private readonly List<CodeInstruction> instructions;
            public OpAction_Insert( int relativeIndex, List<CodeInstruction> instructions ) : base( relativeIndex ) { this.instructions = instructions; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                this.InsertOps( codes, startAt + this.relativeIndex, this.instructions );
            }
        }

        // Overwrite the opcode to Nop and operand to null on a series of instructions, effectively removing them but without affecting labels
        public class OpAction_Remove : OpAction {
            private readonly int removeCount;
            public OpAction_Remove( int relativeIndex, int removeCount ) : base( relativeIndex ) { this.removeCount = removeCount; }
            public override void Do( List<CodeInstruction> codes, int startAt ) {
                this.RemoveOps( codes, startAt + this.relativeIndex, this.removeCount );
            }
        }

    }
}

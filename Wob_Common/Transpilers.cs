using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Wob_Common {
    // Class to hold sets of OpCodes for use in transpiler tests
    internal static class OpCodeSet {
        public static readonly OpCode[] Ldarg = { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldarg_S, OpCodes.Ldarg };
        public static readonly OpCode[] LdLoc = { OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Ldloc_S, OpCodes.Ldloc };
        public static readonly OpCode[] StLoc = { OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Stloc_S, OpCodes.Stloc };
        public static readonly OpCode[] Ldc_I4 = { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, OpCodes.Ldc_I4_M1, OpCodes.Ldc_I4_S, OpCodes.Ldc_I4 };
        public static readonly OpCode[] Ldc_I4_Bool = { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1 };

        public static readonly OpCode[] OperandType_FieldInfo = { OpCodes.Ldfld, OpCodes.Ldsfld, OpCodes.Stfld, OpCodes.Stsfld };
        public static readonly OpCode[] OperandType_MethodInfo = { OpCodes.Call, OpCodes.Callvirt, OpCodes.Ldftn, OpCodes.Ldvirtftn };
        public static readonly OpCode[] OperandType_ConstructorInfo = { OpCodes.Newobj };
        public static readonly OpCode[] OperandType_Type = { OpCodes.Isinst };
    }

    // Class for making transpilers easier and more readable
    internal class WobTranspiler {
        private readonly InstructionList instructions;
        private readonly OpTestBlock opTestBlock;
        private readonly List<OpAction> actions;

        // Field showing how many times the block has been matched and actions implemented
        public int NumPatched { get; private set; }

        public WobTranspiler( IEnumerable<CodeInstruction> instructions, List<OpTestLine> tests, List<OpAction> actions ) {
            this.instructions = new InstructionList( instructions );
            this.opTestBlock = new OpTestBlock( tests );
            this.actions = actions;
            this.NumPatched = 0;
        }

        // Apply the patch actions for each occurrence of the matched lines
        public IEnumerable<CodeInstruction> PatchAll() {
            WobPlugin.Log( "Starting search for instructions" );
            // Start at the beginning of the method
            int index = -1;
            // Find the next occurrence
            while( this.opTestBlock.GetNext( this.instructions.GetList(), ref index ) ) {
                WobPlugin.Log( "Occurrence found at " + index + " - patching" );
                // Perform each of the patching actions in turn
                foreach( OpAction action in this.actions ) {
                    action.Do( this.instructions, index );
                }
                // Record that an occurrence has been patched
                this.NumPatched++;
            }
            WobPlugin.Log( this.NumPatched + " occurrence" + ( this.NumPatched == 1 ? "" : "s" ) + " patched" );
            // Return the modified instructions
            return this.instructions.GetResult();
        }

        // Apply the patch actions for the first occurrence of the matched lines
        public IEnumerable<CodeInstruction> PatchFirst() { return this.PatchOnce( 1 ); }

        // Apply the patch actions for the nth occurrence of the matched lines
        public IEnumerable<CodeInstruction> PatchOnce( int occurrence ) {
            WobPlugin.Log( "Starting search for instructions" );
            // Start at the beginning of the method
            int index = -1;
            // Keep track of how many occurrences have been found so far
            int occurrenceCount = 0;
            // Find the next occurrence
            while( this.opTestBlock.GetNext( this.instructions.GetList(), ref index ) ) {
                // Increment the counter, then check id it matches the parameter
                occurrenceCount++;
                if( occurrenceCount == occurrence ) {
                    WobPlugin.Log( "Occurrence found at " + index + " - patching" );
                    // Perform each of the patching actions in turn
                    foreach( OpAction action in this.actions ) {
                        action.Do( this.instructions, index );
                    }
                    // Record that an occurrence has been patched
                    this.NumPatched++;
                }
            }
            WobPlugin.Log( this.NumPatched + " occurrence" + ( this.NumPatched == 1 ? "" : "s" ) + " patched" );
            // Return the modified instructions
            return this.instructions.GetResult();
        }

        // Class to act as a wrapper for the list of instructions and provide some useful access methods
        public class InstructionList {
            // The internal code list
            private readonly List<CodeInstruction> codes;

            // Constructor that takes the enumerable from a transpiler parameter and turns it into a list
            public InstructionList( IEnumerable<CodeInstruction> codes ) { this.codes = new List<CodeInstruction>( codes ); }

            // Turn the list back into an enumerable for the transpiler return value
            public IEnumerable<CodeInstruction> GetResult() { return this.codes.AsEnumerable(); }

            // Get the codes list
            public List<CodeInstruction> GetList() { return this.codes; }

            // Get the instruction at a specific index in the list
            public CodeInstruction GetOp( int index ) {
                return 0 <= index && index < this.codes.Count ? this.codes[index] : null;
            }

            // Overwrite the opcode for an instruction at a specific index in the list
            public void SetOp( int index, OpCode opcode ) {
                if( 0 <= index && index < this.codes.Count ) {
                    this.codes[index].opcode = opcode;
                }
            }
            // Overwrite the opcode and operand for an instruction at a specific index in the list
            public void SetOp( int index, OpCode opcode, object operand ) {
                if( 0 <= index && index < this.codes.Count ) {
                    this.codes[index].opcode = opcode;
                    this.codes[index].operand = operand;
                }
            }
            // Overwrite the instruction at a specific index in the list
            public void SetOp( int index, CodeInstruction instruction ) {
                if( 0 <= index && index < this.codes.Count ) {
                    this.codes[index] = instruction;
                }
            }

            // Overwrite the opcode and operand with a non-functional operation for an instruction at a specific index in the list
            public void RemoveOp( int index ) { this.SetOp( index, OpCodes.Nop, null ); }

            // Overwite all instructions in a range with non-functional operations
            public void RemoveOps( int startAt, int count ) {
                if( 0 <= startAt && startAt < ( this.codes.Count - count ) ) {
                    for( int i = 0; i < count; i++ ) {
                        this.RemoveOp( startAt + i );
                    }
                }
            }

            // Insert a new instruction at the specified index
            public void InsertOp( int index, OpCode opcode, object operand = null ) { this.InsertOp( index, new CodeInstruction( opcode, operand ) ); }
            // Insert a new instruction at the specified index
            public void InsertOp( int index, CodeInstruction instruction ) {
                if( 0 <= index && index < this.codes.Count ) {
                    this.codes.Insert( index, instruction );
                }
            }

            // Insert a set of new instructions at the specified index
            public void InsertOps( int index, List<CodeInstruction> instructions ) {
                if( 0 <= index && index < this.codes.Count ) {
                    this.codes.InsertRange( index, instructions );
                }
            }
        }

        // Class for a set of tests to be searched for in the instruction list
        public class OpTestBlock {
            private readonly List<OpTestLine> tests;

            public OpTestBlock( List<OpTestLine> tests ) {
                this.tests = tests;
            }

            // Get the number of instruction tests in this block
            public int Count { get { return this.tests.Count; } }

            // Check whether the tests in this block match the instruction list at the specified starting index
            public bool Test( List<CodeInstruction> codes, int startAt ) {
                bool found = true;
                // Make sure we don't go out of bounds
                if( 0 <= startAt && startAt < ( codes.Count - this.tests.Count ) ) {
                    // Go through the list of tests
                    for( int i = 0; i < this.tests.Count; i++ ) {
                        // Check if the current instruction matches the test
                        if( !this.tests[i].IsOp( codes[startAt + i] ) ) {
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

            // Find the next instance of the block in the instructions, if it exists, and return the index where it is found
            public bool GetNext( List<CodeInstruction> codes, ref int index ) {
                // Make sure we don't go out of bounds, and we don't loop infinitely
                index = index < 0 ? 0 : ( index + 1 );
                // Go through the instruction list
                for( int i = index; i < ( codes.Count - this.tests.Count ); i++ ) {
                    // Check if block matches the current index in the instruction list
                    if( this.Test( codes, i ) ) {
                        // Found, so set the index and return true
                        index = i;
                        return true;
                    }
                }
                // Not found, so set an invalid index
                index = -1;
                return false;
            }

            // Get the index of the last line of the matched block
            public int LastIndex( int startAt ) { return startAt + this.tests.Count - 1; }
        }

        // Class for a test against a single instruction
        public class OpTestLine {
            private readonly OpCode[] opcodes;
            private readonly object operand;
            private readonly Type type;
            private readonly string name;

            // Constructors taking all conditions that the instruction must satisfy to pass the test - params are null for no checks
            public OpTestLine( OpCode opcode, object operand = null, Type type = null, string name = null ) : this( new OpCode[] { opcode }, operand, type, name ) { }
            public OpTestLine( OpCode[] opcodes, object operand = null, Type type = null, string name = null ) {
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
                WobPlugin.Log( "ERROR: No operand comparison implemented for " + code.opcode );
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
                WobPlugin.Log( "ERROR: No type comparison implemented for " + code.opcode );
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
                WobPlugin.Log( "ERROR: No name comparison implemented for " + code.opcode );
                return false;
            }
        }

        public abstract class OpAction {
            protected readonly int relativeIndex;
            protected OpAction( int relativeIndex ) { this.relativeIndex = relativeIndex; }
            public abstract void Do( InstructionList codes, int startAt );
        }

        public class OpAction_SetOpcode : OpAction {
            private readonly OpCode opcode;
            public OpAction_SetOpcode( int relativeIndex, OpCode opcode ) : base( relativeIndex ) { this.opcode = opcode; }
            public override void Do( InstructionList codes, int startAt ) {
                codes.SetOp( startAt + this.relativeIndex, this.opcode );
            }
        }

        public class OpAction_SetOperand : OpAction {
            private readonly object operand;
            public OpAction_SetOperand( int relativeIndex, object operand ) : base( relativeIndex ) { this.operand = operand; }
            public override void Do( InstructionList codes, int startAt ) {
                int index = startAt + this.relativeIndex;
                codes.SetOp( index, codes.GetOp( index ).opcode, this.operand );
            }
        }

        public class OpAction_SetOperation : OpAction {
            private readonly OpCode opcode;
            private readonly object operand;
            public OpAction_SetOperation( int relativeIndex, OpCode opcode, object operand ) : base( relativeIndex ) { this.opcode = opcode; this.operand = operand; }
            public override void Do( InstructionList codes, int startAt ) {
                codes.SetOp( startAt + this.relativeIndex, this.opcode, this.operand );
            }
        }

        public class OpAction_SetInstruction : OpAction {
            private readonly CodeInstruction instruction;
            public OpAction_SetInstruction( int relativeIndex, CodeInstruction instruction ) : base( relativeIndex ) { this.instruction = instruction; }
            public override void Do( InstructionList codes, int startAt ) {
                codes.SetOp( startAt + this.relativeIndex, this.instruction );
            }
        }

        public class OpAction_SetInstructions : OpAction {
            private readonly List<CodeInstruction> instructions;
            public OpAction_SetInstructions( int relativeIndex, List<CodeInstruction> instructions ) : base( relativeIndex ) { this.instructions = instructions; }
            public override void Do( InstructionList codes, int startAt ) {
                for( int i = 0; i < this.instructions.Count; i++ ) {
                    codes.SetOp( startAt + this.relativeIndex + i, this.instructions[i] );
                }
            }
        }

        public class OpAction_Insert : OpAction {
            private readonly List<CodeInstruction> instructions;
            public OpAction_Insert( int relativeIndex, List<CodeInstruction> instructions ) : base( relativeIndex ) { this.instructions = instructions; }
            public override void Do( InstructionList codes, int startAt ) {
                codes.InsertOps( startAt + this.relativeIndex, this.instructions );
            }
        }

        public class OpAction_Remove : OpAction {
            private readonly int removeCount;
            public OpAction_Remove( int relativeIndex, int removeCount ) : base( relativeIndex ) { this.removeCount = removeCount; }
            public override void Do( InstructionList codes, int startAt ) {
                codes.RemoveOps( startAt + this.relativeIndex, this.removeCount );
            }
        }

    }
}

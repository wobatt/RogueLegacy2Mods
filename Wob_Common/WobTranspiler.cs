using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Wob_Common {
    /// <summary>
    /// Static class holding sets of OpCodes for use in transpiler tests.
    /// </summary>
    public static class OpCodeSet {
        // Most of these are to combine int32 and int8 variants, as what dnSpy/ILSpy displays and what the transpiler detects are not always the same
        public static readonly HashSet<OpCode> Beq     = new HashSet<OpCode> { OpCodes.Beq,     OpCodes.Beq_S     };
        public static readonly HashSet<OpCode> Bge     = new HashSet<OpCode> { OpCodes.Bge,     OpCodes.Bge_S     };
        public static readonly HashSet<OpCode> Bge_Un  = new HashSet<OpCode> { OpCodes.Bge_Un,  OpCodes.Bge_Un_S  };
        public static readonly HashSet<OpCode> Bgt     = new HashSet<OpCode> { OpCodes.Bgt,     OpCodes.Bgt_S     };
        public static readonly HashSet<OpCode> Bgt_Un  = new HashSet<OpCode> { OpCodes.Bgt_Un,  OpCodes.Bgt_Un_S  };
        public static readonly HashSet<OpCode> Ble     = new HashSet<OpCode> { OpCodes.Ble,     OpCodes.Ble_S     };
        public static readonly HashSet<OpCode> Ble_Un  = new HashSet<OpCode> { OpCodes.Ble_Un,  OpCodes.Ble_Un_S  };
        public static readonly HashSet<OpCode> Blt     = new HashSet<OpCode> { OpCodes.Blt,     OpCodes.Blt_S     };
        public static readonly HashSet<OpCode> Blt_Un  = new HashSet<OpCode> { OpCodes.Blt_Un,  OpCodes.Blt_Un_S  };
        public static readonly HashSet<OpCode> Bne_Un  = new HashSet<OpCode> { OpCodes.Bne_Un,  OpCodes.Bne_Un_S  };
        public static readonly HashSet<OpCode> Br      = new HashSet<OpCode> { OpCodes.Br,      OpCodes.Br_S      };
        public static readonly HashSet<OpCode> Brfalse = new HashSet<OpCode> { OpCodes.Brfalse, OpCodes.Brfalse_S };
        public static readonly HashSet<OpCode> Brtrue  = new HashSet<OpCode> { OpCodes.Brtrue,  OpCodes.Brtrue_S  };
        public static readonly HashSet<OpCode> Ldarg   = new HashSet<OpCode> { OpCodes.Ldarg,   OpCodes.Ldarg_S,  OpCodes.Ldarg_0,  OpCodes.Ldarg_1,  OpCodes.Ldarg_2,  OpCodes.Ldarg_3 };
        public static readonly HashSet<OpCode> Ldarga  = new HashSet<OpCode> { OpCodes.Ldarga,  OpCodes.Ldarga_S  };
        public static readonly HashSet<OpCode> Ldc_I4  = new HashSet<OpCode> { OpCodes.Ldc_I4,  OpCodes.Ldc_I4_S, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, OpCodes.Ldc_I4_M1 };
        public static readonly HashSet<OpCode> Ldloc   = new HashSet<OpCode> { OpCodes.Ldloc,   OpCodes.Ldloc_S,  OpCodes.Ldloc_0,  OpCodes.Ldloc_1,  OpCodes.Ldloc_2,  OpCodes.Ldloc_3 };
        public static readonly HashSet<OpCode> Ldloca  = new HashSet<OpCode> { OpCodes.Ldloca,  OpCodes.Ldloca_S  };
        public static readonly HashSet<OpCode> Leave   = new HashSet<OpCode> { OpCodes.Leave,   OpCodes.Leave_S   };
        public static readonly HashSet<OpCode> Starg   = new HashSet<OpCode> { OpCodes.Starg,   OpCodes.Starg_S   };
        public static readonly HashSet<OpCode> Stloc   = new HashSet<OpCode> { OpCodes.Stloc,   OpCodes.Stloc_S,  OpCodes.Stloc_0,  OpCodes.Stloc_1,  OpCodes.Stloc_2,  OpCodes.Stloc_3 };
        // Boolean literals are 0 (false) or 1 (true), so this is for them 
        public static readonly HashSet<OpCode> Ldc_I4_Bool = new HashSet<OpCode> { OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1 };
        // These are grouped by operand type, mostly used in my transpiler class
        public static readonly HashSet<OpCode> OperandType_FieldInfo       = new HashSet<OpCode> { OpCodes.Ldfld, OpCodes.Ldsfld,   OpCodes.Stfld, OpCodes.Stsfld    };
        public static readonly HashSet<OpCode> OperandType_MethodInfo      = new HashSet<OpCode> { OpCodes.Call,  OpCodes.Callvirt, OpCodes.Ldftn, OpCodes.Ldvirtftn };
        public static readonly HashSet<OpCode> OperandType_ConstructorInfo = new HashSet<OpCode> { OpCodes.Newobj };
        public static readonly HashSet<OpCode> OperandType_Type            = new HashSet<OpCode> { OpCodes.Isinst };
    }

    /// <summary>
    /// Class for making transpilers easier and more readable.
    /// </summary>
    public class WobTranspiler {
        /// <summary>
        /// The method instructions to be patched.
        /// </summary>
        public List<CodeInstruction> CodeList { get; private set; }

        /// <summary>
        /// Total number of times an instruction block has been matched and actions implemented.
        /// </summary>
        public int NumPatched { get; private set; }

        /// <summary>
        /// Initialises a transpiler handler for a method's instructions.
        /// </summary>
        /// <param name="instructions">The method instructions to be patched.</param>
        public WobTranspiler( IEnumerable<CodeInstruction> instructions ) {
            this.CodeList = new List<CodeInstruction>( instructions );
            this.NumPatched = 0;
        }

        /// <summary>
        /// Get the results of the applied patches.
        /// </summary>
        /// <returns>Patched method instructions.</returns>
        public IEnumerable<CodeInstruction> GetResult() {
            return this.CodeList.AsEnumerable();
        }

        /// <summary>
        /// Apply the patch actions for all occurrences of the matched lines.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="actions">The patching actions to take when matched.</param>
        public void PatchAll( List<OpTest> tests, List<OpAction> actions ) { this.Patch( tests, actions, -1 ); }

        /// <summary>
        /// Apply the patch actions for the first occurrence of the matched lines.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="actions">The patching actions to take when matched.</param>
        public void PatchFirst( List<OpTest> tests, List<OpAction> actions ) { this.Patch( tests, actions, 1 ); }

        /// <summary>
        /// Apply the patch actions for the nth occurrence of the matched lines.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="actions">The patching actions to take when matched.</param>
        /// <param name="occurrence">The occurrence to be patched. Must be greater than 0.</param>
        public void PatchOnce( List<OpTest> tests, List<OpAction> actions, int occurrence ) { this.Patch( tests, actions, occurrence <= 0 ? 1 : occurrence ); }

        /// <summary>
        /// Apply the patch actions for matched lines.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="actions">The patching actions to take when matched.</param>
        /// <param name="occurrence">The occurrence to be patched. Values of 0 or less mean patch all occurrences.</param>
        public void Patch( List<OpTest> tests, List<OpAction> actions, int occurrence = 1 ) {
            bool patchAll = occurrence <= 0;
            int patchCount = 0;
            WobPlugin.Log( "Starting search for instructions" );
            // Start at the beginning of the method
            int index = -1;
            // Keep track of how many occurrences have been found so far
            int occurrenceCount = 0;
            // Find the next occurrence
            while( this.GetNext( tests, ref index ) ) {
                // Increment the counter, then check id it matches the parameter
                occurrenceCount++;
                if( patchAll || ( occurrenceCount == occurrence ) ) {
                    WobPlugin.Log( "Occurrence found at " + index + " - patching" );
                    // Perform each of the patching actions in turn
                    foreach( OpAction action in actions ) {
                        action.Do( this.CodeList, ref index );
                    }
                    // Record that an occurrence has been patched
                    patchCount++;
                    this.NumPatched++;
                }
            }
            WobPlugin.Log( patchCount + " occurrence" + ( patchCount == 1 ? "" : "s" ) + " patched" );
        }

        /// <summary>
        /// Find the index of the next instance of the block in the method instructions, if it exists.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="index">Index of previous match to start at, outputting the index of the next match or -1 if no match is found.</param>
        /// <returns>Returns <see langword="true"/> if a match has been found, otherwise <see langword="false"/>.</returns>
        private bool GetNext( List<OpTest> tests, ref int index ) {
            // Make sure we don't go out of bounds, and we don't loop infinitely
            index = index < 0 ? 0 : ( index + 1 );
            // Go through the instruction list
            for( int i = index; i < ( this.CodeList.Count - tests.Count ); i++ ) {
                // Check if block matches the current index in the instruction list
                if( this.Test( tests, i ) ) {
                    // Found, so set the index and return true
                    index = i;
                    return true;
                }
            }
            // Not found, so set an invalid index
            index = -1;
            return false;
        }

        /// <summary>
        /// Check whether the tests in this block match the method instructions at the specified starting index.
        /// </summary>
        /// <param name="tests">The instructions to be matched.</param>
        /// <param name="startAt">Index in the method instructions to start testing at.</param>
        /// <returns>Returns <see langword="true"/> if all conditions are met, otherwise <see langword="false"/>.</returns>
        private bool Test( List<OpTest> tests, int startAt ) {
            bool found = true;
            // Make sure we don't go out of bounds
            if( 0 <= startAt && startAt < ( this.CodeList.Count - tests.Count ) ) {
                // Go through the list of tests
                for( int i = 0; i < tests.Count; i++ ) {
                    // Check if the current instruction matches the test
                    if( !tests[i].IsOp( this.CodeList[startAt + i] ) ) {
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

        /// <summary>
        /// Class for matching a single instruction against specified criteria.
        /// </summary>
        public class OpTest {
            private readonly HashSet<OpCode> opcodes;
            private readonly object operand;
            private readonly Type type;
            private readonly string name;

            /// <summary>
            /// Initialise the test conditions that the instruction must satisfy to pass the test.
            /// </summary>
            /// <param name="opcode">Specific opcode that must be present.</param>
            /// <param name="operand">Operand value. Must be an equatable type. Passing null means do not test.</param>
            /// <param name="type">Declaring type of a field, method, or constructor in the operand. Passing null means do not test.</param>
            /// <param name="name">Name of a field, method, or constructor in the operand. Passing null means do not test.</param>
            public OpTest( OpCode opcode, object operand = null, Type type = null, string name = null ) : this( new HashSet<OpCode> { opcode }, operand, type, name ) { }
            /// <summary>
            /// Initialise the test conditions that the instruction must satisfy to pass the test.
            /// </summary>
            /// <param name="opcodes">Set of opcodes, one of which must be present. Passing null means do not test.</param>
            /// <param name="operand">Operand value. Must be an equatable type. Passing null means do not test.</param>
            /// <param name="type">Declaring type of a field, method, or constructor in the operand. Passing null means do not test.</param>
            /// <param name="name">Name of a field, method, or constructor in the operand. Passing null means do not test.</param>
            public OpTest( HashSet<OpCode> opcodes = null, object operand = null, Type type = null, string name = null ) {
                this.opcodes = opcodes;
                this.operand = operand;
                this.type = type;
                this.name = name;
            }

            /// <summary>
            /// Test an instruction against the conditions.
            /// </summary>
            /// <param name="code">The instruction to be tested.</param>
            /// <returns>Returns <see langword="true"/> if all conditions are met, otherwise <see langword="false"/>.</returns>
            public bool IsOp( CodeInstruction code ) {
                return ( this.opcodes == null || this.opcodes.Contains( code.opcode ) ) && ( this.operand == null || this.HasOperand( code ) ) && ( this.name == null || this.HasName( code ) ) && ( this.type == null || this.HasType( code ) );
            }

            // Just a list of the primitive numeric types
            private static readonly HashSet<Type> numericTypes = new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) };
            // The Ldc_I4 opcodes that put specific values onto the stack, and the number they add, for operand comparison
            private static readonly Dictionary<OpCode,int> i4NoOperands = new Dictionary<OpCode, int> {
                { OpCodes.Ldc_I4_0, 0 }, { OpCodes.Ldc_I4_1, 1 }, { OpCodes.Ldc_I4_2, 2 }, { OpCodes.Ldc_I4_3, 3 }, { OpCodes.Ldc_I4_4, 4 },
                { OpCodes.Ldc_I4_5, 5 }, { OpCodes.Ldc_I4_6, 6 }, { OpCodes.Ldc_I4_7, 7 }, { OpCodes.Ldc_I4_8, 8 }, { OpCodes.Ldc_I4_M1, -1 },
            };

            // Check if the object is of the desired type, or is numeric and can be cast to it
            private static bool TryTypeCast<T>( object objToCast, out T testValue ) {
                testValue = default;
                Type operandType = objToCast.GetType();
                if( operandType == typeof( T ) ) {
                    testValue = (T)objToCast;
                    return true;
                } else {
                    if( numericTypes.Contains( typeof( T ) ) && ( numericTypes.Contains( operandType ) || operandType.IsEnum ) ) {
                        try {
                            testValue = (T)Convert.ChangeType( objToCast, typeof( T ) );
                            return true;
                        } catch( OverflowException ) {
                            testValue = default;
                            return false;
                        }
                    }
                }
                return false;
            }

            // Compare operands - needs to be instruction OpCode dependant as these define the type of the operand
            private bool HasOperand( CodeInstruction code ) {
                if( code.operand != null ) {
                    Type operandType = this.operand.GetType();
                    try {
                        /* 32-bit integer */ if( code.opcode == OpCodes.Ldc_I4   && TryTypeCast( this.operand, out int    intCast    ) ) { return ( (int)code.operand    ) == intCast;    }
                        /* 8-bit integer  */ if( code.opcode == OpCodes.Ldc_I4_S && TryTypeCast( this.operand, out sbyte  sbyteCast  ) ) { return ( (sbyte)code.operand  ) == sbyteCast;  }
                        /* 64-bit integer */ if( code.opcode == OpCodes.Ldc_I8   && TryTypeCast( this.operand, out long   longCast   ) ) { return ( (long)code.operand   ) == longCast;   }
                        /* 32-bit float   */ if( code.opcode == OpCodes.Ldc_R4   && TryTypeCast( this.operand, out float  floatCast  ) ) { return ( (float)code.operand  ) == floatCast;  }
                        /* 64-bit float   */ if( code.opcode == OpCodes.Ldc_R8   && TryTypeCast( this.operand, out double doubleCast ) ) { return ( (double)code.operand ) == doubleCast; }
                        /* String         */ if( code.opcode == OpCodes.Ldstr    && TryTypeCast( this.operand, out string stringCast ) ) { return ( (string)code.operand ) == stringCast; }
                        /* Fields         */ if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode )       && operandType == typeof( FieldInfo       ) ) { return ( (FieldInfo)code.operand       ) == ( (FieldInfo)this.operand       ); }
                        /* Methods        */ if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode )      && operandType == typeof( MethodInfo      ) ) { return ( (MethodInfo)code.operand      ) == ( (MethodInfo)this.operand      ); }
                        /* Constructors   */ if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) && operandType == typeof( ConstructorInfo ) ) { return ( (ConstructorInfo)code.operand ) == ( (ConstructorInfo)this.operand ); }
                        /* Types          */ if( OpCodeSet.OperandType_Type.Contains( code.opcode )            && operandType == typeof( Type            ) ) { return ( (Type)code.operand            ) == ( (Type)this.operand            ); }
                        /* I4 operandless */ if( i4NoOperands.ContainsKey( code.opcode ) && TryTypeCast( this.operand, out sbyte oplessCast ) ) { return i4NoOperands[code.opcode] == oplessCast; }
                        // No implemented yet - log it then return no match
                        WobPlugin.Log( "ERROR: Could not perform operand comparison for " + code.opcode + " and " + operandType, WobPlugin.ERROR );
                    } catch( InvalidCastException ) {
                        WobPlugin.Log( "ERROR: Wrong operand type for " + code.opcode + " of " + operandType, WobPlugin.ERROR );
                    }
                }
                return false;
            }

            // Compare the declaring type on instructions
            private bool HasType( CodeInstruction code ) {
                if( code.operand != null ) {
                    /* Fields         */ if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode )       ) { return ( (FieldInfo)code.operand ).DeclaringType       == this.type; }
                    /* Methods        */ if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode )      ) { return ( (MethodInfo)code.operand ).DeclaringType      == this.type; }
                    /* Constructors   */ if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) ) { return ( (ConstructorInfo)code.operand ).DeclaringType == this.type; }
                    /* Types          */ if( OpCodeSet.OperandType_Type.Contains( code.opcode )            ) { return ( (Type)code.operand )                          == this.type; }
                    // No implemented yet - log it then return no match
                    WobPlugin.Log( "ERROR: Could not perform type comparison for " + code.opcode, WobPlugin.ERROR );
                }
                return false;
            }

            // Compare the name of fields and methods on instructions
            private bool HasName( CodeInstruction code ) {
                if( code.operand != null ) {
                    /* Fields         */ if( OpCodeSet.OperandType_FieldInfo.Contains( code.opcode )       ) { return ( (FieldInfo)code.operand ).Name       == this.name; }
                    /* Methods        */ if( OpCodeSet.OperandType_MethodInfo.Contains( code.opcode )      ) { return ( (MethodInfo)code.operand ).Name      == this.name; }
                    /* Constructors   */ if( OpCodeSet.OperandType_ConstructorInfo.Contains( code.opcode ) ) { return ( (ConstructorInfo)code.operand ).Name == this.name; }
                    // No implemented yet - log it then return no match
                    WobPlugin.Log( "ERROR: Could not perform name comparison for " + code.opcode, WobPlugin.ERROR );
                }
                return false;
            }
        }

        /// <summary>
        /// Base class for patching actions that includes standard interactions overridden by subclasses.
        /// </summary>
        public abstract class OpAction {
            /// <summary>
            /// Index relative to the start of the matched code block for where to apply this action.
            /// </summary>
            protected readonly int relativeIndex;

            /// <summary>
            /// Initialise the action by setting the relative index.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            protected OpAction( int relativeIndex ) { this.relativeIndex = relativeIndex; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public abstract void Do( List<CodeInstruction> codes, ref int startAt );

            /// <summary>
            /// Overwrite the opcode for an instruction at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index of the method instruction to be patched.</param>
            /// <param name="opcode">New opcode to be set.</param>
            protected void SetOp( List<CodeInstruction> codes, int index, OpCode opcode ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index].opcode = opcode;
                }
            }

            /// <summary>
            /// Overwrite the opcode and operand for an instruction at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index of the method instruction to be patched.</param>
            /// <param name="opcode">New opcode to be set.</param>
            /// <param name="operand">New operand to be set.</param>
            protected void SetOp( List<CodeInstruction> codes, int index, OpCode opcode, object operand ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index].opcode = opcode;
                    codes[index].operand = operand;
                }
            }

            /// <summary>
            /// Overwrite the instruction at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index of the method instruction to be patched.</param>
            /// <param name="instruction">New instruction to be set.</param>
            protected void SetOp( List<CodeInstruction> codes, int index, CodeInstruction instruction ) {
                if( 0 <= index && index < codes.Count ) {
                    codes[index] = instruction;
                }
            }

            /// <summary>
            /// Overwrite am instruction opcode and operand with a non-functional operation at the specified index, effectively removing it while preserving labels.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index of the method instruction to be patched.</param>
            protected void RemoveOp( List<CodeInstruction> codes, int index ) { this.SetOp( codes, index, OpCodes.Nop, null ); }

            /// <summary>
            /// Overwite all instructions in a range with non-functional operations, effectively removing them while preserving labels.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the method instruction to start from.</param>
            /// <param name="count">Number of method instructions to be removed.</param>
            protected void RemoveOps( List<CodeInstruction> codes, int startAt, int count ) {
                if( 0 <= startAt && startAt < ( codes.Count - count ) ) {
                    for( int i = 0; i < count; i++ ) {
                        this.RemoveOp( codes, startAt + i );
                    }
                }
            }

            /// <summary>
            /// Insert a new instruction at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index the method instructions to insert at.</param>
            /// <param name="opcode">Opcode of the new instruction to be inserted.</param>
            /// <param name="operand">Operand of the new instruction to be inserted.</param>
            protected void InsertOp( List<CodeInstruction> codes, int index, OpCode opcode, object operand = null ) { this.InsertOp( codes, index, new CodeInstruction( opcode, operand ) ); }
            /// <summary>
            /// Insert a new instruction at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index the method instructions to insert at.</param>
            /// <param name="instruction">New instruction to be inserted.</param>
            protected void InsertOp( List<CodeInstruction> codes, int index, CodeInstruction instruction ) {
                if( 0 <= index && index < codes.Count ) {
                    codes.Insert( index, instruction );
                }
            }

            /// <summary>
            /// Insert a set of new instructions at the specified index.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="index">Index the method instructions to insert at.</param>
            /// <param name="instructions">New instructions to be inserted.</param>
            protected void InsertOps( List<CodeInstruction> codes, int index, List<CodeInstruction> instructions ) {
                if( 0 <= index && index < codes.Count ) {
                    codes.InsertRange( index, instructions );
                }
            }
        }

        /// <summary>
        /// Overwrite the opcode on an instruction.
        /// </summary>
        public class OpAction_SetOpcode : OpAction {
            private readonly OpCode opcode;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the opcode to be set.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="opcode">New opcode to be set.</param>
            public OpAction_SetOpcode( int relativeIndex, OpCode opcode ) : base( relativeIndex ) { this.opcode = opcode; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                this.SetOp( codes, startAt + this.relativeIndex, this.opcode );
            }
        }

        /// <summary>
        /// Overwrite the operand on an instruction
        /// </summary>
        public class OpAction_SetOperand : OpAction {
            private readonly object operand;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the operand to be set.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="operand">New operand to be set.</param>
            public OpAction_SetOperand( int relativeIndex, object operand ) : base( relativeIndex ) { this.operand = operand; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                int index = startAt + this.relativeIndex;
                this.SetOp( codes, index, codes[index].opcode, this.operand );
            }
        }

        /// <summary>
        /// Overwrite an instruction. Be careful with this as the instruction may have a label on it.
        /// </summary>
        public class OpAction_SetInstruction : OpAction {
            private readonly CodeInstruction instruction;
            private readonly bool safe;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the opcode and operand to be set.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="opcode">New opcode to be set.</param>
            /// <param name="operand">New operand to be set.</param>
            public OpAction_SetInstruction( int relativeIndex, OpCode opcode, object operand ) : this( relativeIndex, new CodeInstruction( opcode, operand ), true ) { }
            /// <summary>
            /// Initialise the action with the relative index to be patched and the instruction to be set.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instruction">New instruction to be set.</param>
            /// <param name="safe">If <see langword="true"/> this only overwrites the opcode and operand of the instruction, preserving labels. If <see langword="false"/> the the instruction will be entirely replaced - use with caution.</param>
            public OpAction_SetInstruction( int relativeIndex, CodeInstruction instruction, bool safe = true ) : base( relativeIndex ) { this.instruction = instruction; this.safe = safe; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                if( this.safe ) {
                    this.SetOp( codes, startAt + this.relativeIndex, this.instruction.opcode, this.instruction.operand );
                } else {
                    this.SetOp( codes, startAt + this.relativeIndex, this.instruction );
                }
            }
        }

        /// <summary>
        /// Overwrite a series of instructions. Be careful with this as the instructions may have labels on them.
        /// </summary>
        public class OpAction_SetInstructions : OpAction {
            private readonly List<CodeInstruction> instructions;
            private readonly bool safe;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the instructions to be set.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instructions">New instructions to be inserted.</param>
            /// <param name="safe">If <see langword="true"/> this only overwrites the opcode and operand of each instruction, preserving labels. If <see langword="false"/> the each instruction will be entirely replaced - use with caution.</param>
            public OpAction_SetInstructions( int relativeIndex, List<CodeInstruction> instructions, bool safe = true ) : base( relativeIndex ) { this.instructions = instructions; this.safe = safe; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                for( int i = 0; i < this.instructions.Count; i++ ) {
                    if( this.safe ) {
                        this.SetOp( codes, startAt + this.relativeIndex + i, this.instructions[i].opcode, this.instructions[i].operand );
                    } else {
                        this.SetOp( codes, startAt + this.relativeIndex + i, this.instructions[i] );
                    }
                }
            }
        }

        /// <summary>
        /// Overwite all instructions in a range with non-functional operations, effectively removing them while preserving labels.
        /// </summary>
        public class OpAction_Remove : OpAction {
            private readonly int removeCount;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the number of instructions to be removed.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="removeCount">Number of method instructions to be removed.</param>
            public OpAction_Remove( int relativeIndex, int removeCount ) : base( relativeIndex ) { this.removeCount = removeCount; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                this.RemoveOps( codes, startAt + this.relativeIndex, this.removeCount );
            }
        }

        /// <summary>
        /// Insert a set of new instructions.
        /// </summary>
        public class OpAction_Insert : OpAction {
            private readonly List<CodeInstruction> instructions;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instruction to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="opcode">New opcode to be inserted.</param>
            public OpAction_Insert( int relativeIndex, OpCode opcode ) : this( relativeIndex, new CodeInstruction( opcode, null ) ) { }
            
            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instruction to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="opcode">New opcode to be inserted.</param>
            /// <param name="operand">New operand to be inserted.</param>
            public OpAction_Insert( int relativeIndex, OpCode opcode, object operand ) : this( relativeIndex, new CodeInstruction( opcode, operand ) ) { }

            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instruction to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instruction">New instruction to be inserted.</param>
            public OpAction_Insert( int relativeIndex, CodeInstruction instruction ) : this( relativeIndex, new List<CodeInstruction> { instruction } ) { }

            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instructions to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instructions">New instructions to be inserted.</param>
            public OpAction_Insert( int relativeIndex, List<CodeInstruction> instructions ) : base( relativeIndex ) { this.instructions = instructions; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                this.InsertOps( codes, startAt + this.relativeIndex, this.instructions );
                startAt += this.instructions.Count;
            }
        }

        /// <summary>
        /// Insert a set of new instructions.
        /// </summary>
        public class OpAction_InsertCopy : OpAction {
            private readonly List<OpCopy> instructions;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the instruction to be copied and inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="relativeCopyFrom">Index relative to the start of the matched code block for the instruction to be copied.</param>
            public OpAction_InsertCopy( int relativeIndex, int relativeCopyFrom ) : this( relativeIndex, new List<OpCopy> { new OpCopy( relativeCopyFrom ) } ) { }
            /// <summary>
            /// Initialise the action with the relative index to be patched and the instruction to be copied and inserted, with opcode override.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="relativeCopyFrom">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="opcode">Opcode to be set on the copied instruction.</param>
            public OpAction_InsertCopy( int relativeIndex, int relativeCopyFrom, OpCode opcode ) : this( relativeIndex, new List<OpCopy> { new OpCopy( relativeCopyFrom, opcode ) } ) { }
            /// <summary>
            /// Initialise the action with the relative index to be patched and the instruction to be copied and inserted, with operand override.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="relativeCopyFrom">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="operand">Operand to be set on the copied instruction.</param>
            public OpAction_InsertCopy( int relativeIndex, int relativeCopyFrom, object operand ) : this( relativeIndex, new List<OpCopy> { new OpCopy( relativeCopyFrom, operand ) } ) { }
            /// <summary>
            /// Initialise the action with the relative index to be patched and the instruction to be copied and inserted, with opcode and operand overrides.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="relativeCopyFrom">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="opcode">Opcode to be set on the copied instruction.</param>
            /// <param name="operand">Operand to be set on the copied instruction.</param>
            public OpAction_InsertCopy( int relativeIndex, int relativeCopyFrom, OpCode opcode, object operand ) : this( relativeIndex, new List<OpCopy> { new OpCopy( relativeCopyFrom, opcode, operand ) } ) { }
            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instructions to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instructions">New instructions to be inserted.</param>
            public OpAction_InsertCopy( int relativeIndex, List<OpCopy> instructions ) : base( relativeIndex ) { this.instructions = instructions; }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                List<CodeInstruction> newInstructions = new List<CodeInstruction>();
                foreach( OpCopy instruction in this.instructions ) {
                    newInstructions.Add( instruction.GetInstruction( codes, startAt ) );
                }
                this.InsertOps( codes, startAt + this.relativeIndex, newInstructions );
                startAt += this.instructions.Count;
            }
        }

        /// <summary>
        /// Insert a set of new instructions.
        /// </summary>
        public class OpAction_Extract : OpAction {
            private readonly CodeInstruction instruction;

            /// <summary>
            /// Initialise the action with the relative index to be patched and the new instructions to be inserted.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for where to apply this action.</param>
            /// <param name="instructions">New instructions to be inserted.</param>
            public OpAction_Extract( int relativeIndex, out CodeInstruction instruction ) : base( relativeIndex ) {
                this.instruction = new CodeInstruction( OpCodes.Nop );
                instruction = this.instruction;
            }

            /// <summary>
            /// Apply the action to the code list starting at the specified offset.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            public override void Do( List<CodeInstruction> codes, ref int startAt ) {
                this.instruction.opcode = codes[startAt + this.relativeIndex].opcode;
                this.instruction.operand = codes[startAt + this.relativeIndex].operand;
            }
        }

        /// <summary>
        /// Class to simplify copying and modifying an existing instruction, to be used with an insert copy action.
        /// </summary>
        public class OpCopy {
            private readonly int relativeIndex;
            private readonly bool setOpcode;
            private readonly bool setOperand;
            private readonly CodeInstruction instruction;

            /// <summary>
            /// Initialise with the instruction to be copied, with opcode and operand overrides taken from the instruction.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="instruction">Instruction with the opcode and operand to be set on the copied instruction.</param>
            public OpCopy( int relativeIndex, CodeInstruction instruction ) {
                this.relativeIndex = relativeIndex;
                this.instruction = instruction;
                this.setOpcode = true;
                this.setOperand = true;
            }
            /// <summary>
            /// Initialise with the instruction to be copied.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for the instruction to be copied.</param>
            public OpCopy( int relativeIndex ) : this( relativeIndex, null ) { this.setOpcode = false; this.setOperand = false; }
            /// <summary>
            /// Initialise with the instruction to be copied, with opcode override.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="opcode">Opcode to be set on the copied instruction.</param>
            public OpCopy( int relativeIndex, OpCode opcode ) : this( relativeIndex, new CodeInstruction( opcode ) ) { this.setOperand = false; }
            /// <summary>
            /// Initialise with the instruction to be copied, with operand override.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="operand">Operand to be set on the copied instruction.</param>
            public OpCopy( int relativeIndex, object operand ) : this( relativeIndex, new CodeInstruction( OpCodes.Nop, operand ) ) { this.setOpcode = false; }
            /// <summary>
            /// Initialise with the instruction to be copied, with opcode and operand overrides.
            /// </summary>
            /// <param name="relativeIndex">Index relative to the start of the matched code block for the instruction to be copied.</param>
            /// <param name="opcode">Opcode to be set on the copied instruction.</param>
            /// <param name="operand">Operand to be set on the copied instruction.</param>
            public OpCopy( int relativeIndex, OpCode opcode, object operand ) : this( relativeIndex, new CodeInstruction( opcode, operand ) ) { }
            /// <summary>
            /// Initialise with a new instruction to be inserted with the copied instructions.
            /// </summary>
            /// <param name="opcode">New opcode to be inserted.</param>
            /// <param name="operand">New operand to be inserted.</param>
            public OpCopy( OpCode opcode, object operand ) : this( int.MinValue, new CodeInstruction( opcode, operand ) ) { }
            /// <summary>
            /// Initialise with a new instruction to be inserted with the copied instructions.
            /// </summary>
            /// <param name="instruction">New instruction to be inserted.</param>
            public OpCopy( CodeInstruction instruction ) : this( int.MinValue, instruction ) { }

            /// <summary>
            /// Perform the copy and overrides to get the new instruction to be inserted.
            /// </summary>
            /// <param name="codes">Full method instruction list to be patched.</param>
            /// <param name="startAt">Index of the start of the matched block of instructions.</param>
            /// <returns></returns>
            public CodeInstruction GetInstruction( List<CodeInstruction> codes, int startAt ) {
                if( this.relativeIndex == int.MinValue ) {
                    return new CodeInstruction( this.instruction );
                } else {
                    CodeInstruction instruction = new CodeInstruction( codes[startAt + this.relativeIndex] );
                    if( this.setOpcode ) {
                        instruction.opcode = this.instruction.opcode;
                    }
                    if( this.setOperand ) {
                        instruction.operand = this.instruction.operand;
                    }
                    return instruction;
                }
            }
        }
    }
}

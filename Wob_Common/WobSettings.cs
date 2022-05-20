using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace Wob_Common {
    internal class WobSettings {
        private const string DEFAULT_SECTION = "Options";

        private static ConfigFile configFile;
        private readonly Dictionary<string, Entry> configItems;

        public WobSettings( ConfigFile config ) {
            configFile = config;
            this.configItems = new Dictionary<string, Entry>();
        }
        public WobSettings( ConfigFile config, Entry setting ) : this( config ) { this.Add( setting ); }
        public WobSettings( ConfigFile config, List<Entry> settings ) : this( config ) { this.Add( settings ); }

        public void Add( Entry setting ) {
            this.configItems.Add( setting.Key, setting );
        }

        public void Add( Entry[] settings ) {
            foreach( Entry setting in settings ) {
                this.configItems.Add( setting.Key, setting );
            }
        }

        public void Add( List<Entry> settings ) {
            foreach( Entry setting in settings ) {
                this.configItems.Add( setting.Key, setting );
            }
        }

        public T Get<T>( string name, T defaultValue ) { return this.Get( DEFAULT_SECTION, name, defaultValue ); }
        public T Get<T>( string section, string name, T defaultValue ) {
            Entry item;
            if( this.configItems.TryGetValue( section + "." + name, out item ) ) {
                return item.Get( defaultValue );
            } else {
                WobPlugin.Log( "WARNING: Setting not found for " + section + "." + name );
                return defaultValue;
            }
        }

        public abstract class Entry {
            public string Key         { get; protected set; }
            public string Section     { get; protected set; }
            public string Name        { get; protected set; }
            public string Description { get; protected set; }
            public Type   DataType    { get; protected set; }

            protected Entry( string section, string name, string description, Type dataType ) {
                this.Key = section + "." + name;
                this.Section = section;
                this.Name = name;
                this.Description = description;
                this.DataType = dataType;
            }

            protected abstract object GetValue();

            public T Get<T>( T defaultValue ) {
                T value = defaultValue;
                // Check the data type being requested matches the target data type
                if( typeof( T ) == this.DataType ) {
                    // Request and target are same data type, so return the value
                    value = (T)this.GetValue();
                } else {
                    // Types don't match, so check the list of safe casts of numeric types
                    if( safeCasts.TryGetValue( typeof( T ), out List<Type> safeCastList ) && safeCastList.Contains( this.DataType ) ) {
                        // This is a safe cast, so return the value
                        value = (T)Convert.ChangeType( this.GetValue(), typeof( T ) );
                    } else {
                        // Can't get the value, so log an error, and leave the return value as the default from the parameter
                        WobPlugin.Log( "ERROR: Attempt to get setting " + this.Key + " as " + typeof( T ) + " but it is " + this.DataType, WobPlugin.ERROR );
                    }
                }
                return value;
            }

            private readonly Dictionary<Type, List<Type>> safeCasts = new Dictionary<Type, List<Type>>() {
                // Integer types - can cast from any smaller integer type
                { typeof( short   ), new List<Type> { typeof( sbyte ), typeof( byte ) } },
                { typeof( ushort  ), new List<Type> { typeof( sbyte ), typeof( byte ) } },
                { typeof( int     ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ) } },
                { typeof( uint    ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ) } },
                { typeof( long    ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ) } },
                { typeof( ulong   ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ) } },
                // Floating point types - cast from integers
                { typeof( float   ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ) } },
                { typeof( double  ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ) } },
                { typeof( decimal ), new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ) } },
            };
        }
        
        public class Enum<T> : Entry where T : Enum {
            public T DefaultValue { get; protected set; }
            protected ConfigEntry<T> configEntry;

            public Enum( string name, string description, T value ) : this( DEFAULT_SECTION, name, description, value ) { }
            public Enum( string section, string name, string description, T value ) : base( section, name, description, typeof( T ) ) {
                this.DefaultValue = value;
                this.Bind();
            }

            protected virtual void Bind() {
                this.configEntry = configFile.Bind( this.Section, this.Name, this.DefaultValue, new ConfigDescription( this.Description ) );
            }

            protected override object GetValue() {
                return this.configEntry.Value;
            }
        }

        public class Entry<T> : Entry where T : IComparable, IEquatable<T> {
            public T DefaultValue        { get; protected set; }
            public T[] AcceptedValues    { get; protected set; }
            public (T min, T max) Bounds { get; protected set; }
            public Func<T, T> Limiter    { get; protected set; }
            protected ConfigEntry<T> configEntry;

            public Entry( string name, string description, T value, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( DEFAULT_SECTION, name, description, value, acceptedValues, bounds, limiter ) { }
            public Entry( string section, string name, string description, T value, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : base( section, name, description, typeof(T) ) {
                this.DefaultValue = value;
                this.AcceptedValues = acceptedValues;
                this.Bounds = bounds;
                this.Limiter = limiter;
                this.Bind();
            }

            protected virtual void Bind() {
                this.configEntry = configFile.Bind( this.Section, this.Name, this.DefaultValue, new ConfigDescription( this.Description, this.GetAcceptable() ) );
                if( this.Limiter != null ) {
                    this.configEntry.Value = this.Limiter( this.configEntry.Value );
                }
            }

            protected override object GetValue() {
                return this.configEntry.Value;
            }

            public virtual T Get() {
                return this.configEntry.Value;
            }

            private static readonly List<Type> numericTypes = new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) };
            protected AcceptableValueBase GetAcceptable() {
                AcceptableValueBase acceptableValues = null;
                if( this.AcceptedValues != null ) {
                    acceptableValues = new AcceptableValueList<T>( this.AcceptedValues );
                } else {
                    if( numericTypes.Contains( typeof( T ) ) && this.Bounds.min.CompareTo( this.Bounds.max ) < 0 ) {
                        acceptableValues = new AcceptableValueRange<T>( this.Bounds.min, this.Bounds.max );
                    } else {
                        WobPlugin.Log( "WARNING: No validation for " + this.Key );
                    }
                }
                return acceptableValues;
            }

        }

        public class EntryBool : Entry<bool> {
            public EntryBool( string name, string description, bool value ) : this( DEFAULT_SECTION, name, description, value ) { }
            public EntryBool( string section, string name, string description, bool value ) : base( section, name, description, value, new bool[] { true, false } ) { }
        }

        public class Scaled<T> : Entry<T> where T : IComparable, IEquatable<T> {
            public float Scaler { get; protected set; }

            public Scaled( string name, string description, T value, float scaler, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( DEFAULT_SECTION, name, description, value, scaler, acceptedValues, bounds, limiter ) { }
            public Scaled( string section, string name, string description, T value, float scaler, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : base( section, name, description, value, acceptedValues, bounds, limiter ) {
                this.DataType = typeof( float );
                this.Scaler = scaler;
            }

            protected override void Bind() {
                this.configEntry = configFile.Bind( this.Section, this.Name, this.DefaultValue, new ConfigDescription( this.Description, this.GetAcceptable() ) );
                if( this.Limiter != null ) {
                    this.configEntry.Value = this.Limiter( this.configEntry.Value );
                }
            }

            protected override object GetValue() {
                if( typeof( T ) == typeof( decimal ) ) {
                    return ( (decimal)this.Scaler ) * ( (decimal)Convert.ChangeType( this.configEntry.Value, typeof( decimal ) ) );
                }
                if( typeof( T ) == typeof( double ) ) {
                    return ( (double)this.Scaler ) * ( (double)Convert.ChangeType( this.configEntry.Value, typeof( double ) ) );
                }
                if( typeof( T ) == typeof( float ) || floatCasts.Contains( typeof( T ) ) ) {
                    return this.Scaler * (float)Convert.ChangeType( this.configEntry.Value, typeof( float ) );
                }
                WobPlugin.Log( "ERROR: Could not multiply scaler by type " + typeof( T ), WobPlugin.ERROR );
                return this.configEntry.Value;
            }
            private static readonly List<Type> floatCasts = new List<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ) };
        }

        public class ScaledInt : Scaled<int> {
            public ScaledInt( string name, string description, int value, float scaler, int[] acceptedValues = null, (int min, int max) bounds = default, Func<int, int> limiter = null ) : this( DEFAULT_SECTION, name, description, value, scaler, acceptedValues, bounds, limiter ) { }
            public ScaledInt( string section, string name, string description, int value, float scaler, int[] acceptedValues = null, (int min, int max) bounds = default, Func<int, int> limiter = null ) : base( section, name, description, value, scaler, acceptedValues, bounds, limiter ) { }

            protected override object GetValue() {
                return this.configEntry.Value * this.Scaler;
            }
        }
        
        public class ScaledFloat : Scaled<float> {
            public ScaledFloat( string name, string description, float value, float scaler, float[] acceptedValues = null, (float min, float max) bounds = default, Func<float, float> limiter = null ) : this( DEFAULT_SECTION, name, description, value, scaler, acceptedValues, bounds, limiter ) { }
            public ScaledFloat( string section, string name, string description, float value, float scaler, float[] acceptedValues = null, (float min, float max) bounds = default, Func<float, float> limiter = null ) : base( section, name, description, value, scaler, acceptedValues, bounds, limiter ) { }

            protected override object GetValue() {
                return this.configEntry.Value * this.Scaler;
            }
        }

    }
}

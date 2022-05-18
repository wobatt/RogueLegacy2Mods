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

        private static AcceptableValueBase GetAcceptable<T>( T[] acceptedValues, (T min, T max) bounds ) where T : IComparable, IEquatable<T> {
            AcceptableValueBase acceptableValues = null;
            if( acceptedValues != null ) {
                acceptableValues = new AcceptableValueList<T>( acceptedValues );
            } else {
                if( bounds.Item1.CompareTo( bounds.Item2 ) < 0 ) {
                    acceptableValues = new AcceptableValueRange<T>( bounds.min, bounds.max );
                }
            }
            return acceptableValues;
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

            public abstract object GetValue();

            public T Get<T>( T defaultValue ) {
                T value = defaultValue;
                if( typeof( T ) == this.DataType ) {
                    value = (T)this.GetValue();
                } else {
                    WobPlugin.Log( "ERROR: Attempt to get setting " + this.Key + " as " + typeof( T ) + " but it is " + this.DataType, WobPlugin.ERROR );
                }
                return value;
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
                this.configEntry = configFile.Bind( this.Section, this.Name, this.DefaultValue, new ConfigDescription( this.Description, GetAcceptable( this.AcceptedValues, this.Bounds ) ) );
                if( this.Limiter != null ) {
                    this.configEntry.Value = this.Limiter( this.configEntry.Value );
                }
            }

            public override object GetValue() {
                return this.configEntry.Value;
            }

            public virtual T Get() {
                return this.configEntry.Value;
            }
        }

        public class EntryBool : Entry<bool> {
            public EntryBool( string name, string description, bool value ) : this( DEFAULT_SECTION, name, description, value ) { }
            public EntryBool( string section, string name, string description, bool value ) : base( section, name, description, value, new bool[] { true, false } ) { }
        }

        public abstract class ScaledEntry<T> : Entry<T> where T : IComparable, IEquatable<T> {
            public float Scaler { get; protected set; }

            protected ScaledEntry( string section, string name, string description, T value, float scaler, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : base( section, name, description, value, acceptedValues, bounds, limiter ) {
                this.DataType = typeof( float );
                this.Scaler = scaler;
            }

            protected override void Bind() {
                this.configEntry = WobPlugin.Config.Bind( this.Section, this.Name, this.DefaultValue, new ConfigDescription( this.Description, GetAcceptable( this.AcceptedValues, this.Bounds ) ) );
                if( this.Limiter != null ) {
                    this.configEntry.Value = this.Limiter( this.configEntry.Value );
                }
            }

            public abstract override object GetValue();
        }

        public class ScaledInt : ScaledEntry<int> {
            public ScaledInt( string name, string description, int value, float scaler, int[] acceptedValues = null, (int min, int max) bounds = default, Func<int, int> limiter = null ) : this( DEFAULT_SECTION, name, description, value, scaler, acceptedValues, bounds, limiter ) { }
            public ScaledInt( string section, string name, string description, int value, float scaler, int[] acceptedValues = null, (int min, int max) bounds = default, Func<int, int> limiter = null ) : base( section, name, description, value, scaler, acceptedValues, bounds, limiter ) { }

            public override object GetValue() {
                return this.configEntry.Value * this.Scaler;
            }
        }
        
        public class ScaledFloat : ScaledEntry<float> {
            public ScaledFloat( string name, string description, float value, float scaler, float[] acceptedValues = null, (float min, float max) bounds = default, Func<float, float> limiter = null ) : this( DEFAULT_SECTION, name, description, value, scaler, acceptedValues, bounds, limiter ) { }
            public ScaledFloat( string section, string name, string description, float value, float scaler, float[] acceptedValues = null, (float min, float max) bounds = default, Func<float, float> limiter = null ) : base( section, name, description, value, scaler, acceptedValues, bounds, limiter ) { }

            public override object GetValue() {
                return this.configEntry.Value * this.Scaler;
            }
        }

    }
}

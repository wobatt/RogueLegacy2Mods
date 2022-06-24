using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;

namespace Wob_Common {
    /// <summary>
    /// Class for handling a dictionary of dissimilar configuration entries for a mod.
    /// </summary>
    internal class WobSettings {
        /// <summary>
        /// List of items created in the config file.
        /// </summary>
        private Dictionary<string, Entry> Entries { get; } = new Dictionary<string, Entry>();

        /// <summary>
        /// Static version of 'this'.
        /// </summary>
        private static WobSettings Instance { get; } = new WobSettings();

        /// <summary>
        /// The section name to be used if one isn't specified in the mod.
        /// </summary>
        public const string DEFAULT_SECTION = "Options";

        /// <summary>
        /// Get the combination of section and setting names fomatted in the standard manner used throughout this class and subclasses.
        /// </summary>
        /// <param name="keys">Object defining the section and setting names for the config item.</param>
        /// <returns>Combined section and setting names.</returns>
        private static string GetFullKey( ConfigDefinition keys ) {
            return keys.Section + "." + keys.Key;
        }

        /// <summary>
        /// Register a single config item.
        /// </summary>
        /// <param name="setting">The config item to be added.</param>
        public static void Add( Entry setting ) {
            if( setting != null ) {
                Instance.Entries.Add( setting.FullKey, setting );
            }
        }

        /// <summary>
        /// Register a set of config items.
        /// </summary>
        /// <param name="settings">The config items to be added.</param>
        public static void Add( Entry[] settings ) {
            foreach( Entry setting in settings ) {
                Instance.Entries.Add( setting.FullKey, setting );
            }
        }

        /// <summary>
        /// Register a set of config items.
        /// </summary>
        /// <param name="settings">The config items to be added.</param>
        public static void Add( IEnumerable<Entry> settings ) {
            foreach( Entry setting in settings ) {
                Instance.Entries.Add( setting.FullKey, setting );
            }
        }

        /// <summary>
        /// Check if there is a config item registered for a set of keys.
        /// </summary>
        /// <param name="keys">Object defining the section and setting names for the config item.</param>
        /// <returns>Returns <see langword="true"/> if a setting exists, otherwise <see langword="false"/>.</returns>
        public static bool Exists( ConfigDefinition keys ) {
            if( keys != null ) {
                return Instance.Entries.ContainsKey( GetFullKey( keys ) );
            } else {
                return false;
            }
        }

        /// <summary>
        /// Read the value of a config item.
        /// </summary>
        /// <typeparam name="T">The return type of the value. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="keys">Object defining the section and setting names for the config item.</param>
        /// <param name="defaultValue">The value to be returned if a matching config item cannot be read.</param>
        /// <returns>The value read from the config file, or the default value parameter.</returns>
        public static T Get<T>( ConfigDefinition keys, T defaultValue ) {
            if( keys != null ) {
                if( Instance.Entries.TryGetValue( GetFullKey( keys ), out Entry item ) ) {
                    return item.Get( defaultValue );
                } else {
                    WobPlugin.Log( "WARNING: Setting not found for " + GetFullKey( keys ) );
                    return defaultValue;
                }
            } else {
                WobPlugin.Log( "ERROR: Attempt to get value from null key", WobPlugin.ERROR );
                return defaultValue;
            }
        }
        /// <summary>
        /// Read the value of a config item, using the provided section and setting names.
        /// </summary>
        /// <typeparam name="T">The return type of the value. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="section">The name of the section the config item is in.</param>
        /// <param name="name">The name of the config item.</param>
        /// <param name="defaultValue">The value to be returned if a matching config item cannot be read.</param>
        /// <returns>The value read from the config file, or the default value parameter.</returns>
        public static T Get<T>( string section, string name, T defaultValue ) { return Get( new ConfigDefinition( section, name ), defaultValue ); }
        /// <summary>
        /// Read the value of a config item, using the default section name and a provided setting name.
        /// </summary>
        /// <typeparam name="T">The return type of the value. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="name">The name of the config item.</param>
        /// <param name="defaultValue">The value to be returned if a matching config item cannot be read.</param>
        /// <returns>The value read from the config file, or the default value parameter.</returns>
        public static T Get<T>( string name, T defaultValue ) { return Get( new ConfigDefinition( DEFAULT_SECTION, name ), defaultValue ); }

        /// <summary>
        /// Set the value of a config item.
        /// </summary>
        /// <typeparam name="T">The type of the value being set. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="keys">Object defining the section and setting names for the config item.</param>
        /// <param name="newValue">The value to be written to the config item.</param>
        public static void Set<T>( ConfigDefinition keys, T newValue ) {
            if( keys != null ) {
                if( Instance.Entries.TryGetValue( GetFullKey( keys ), out Entry item ) ) {
                    item.Set( newValue );
                } else {
                    WobPlugin.Log( "ERROR: Setting not found for " + GetFullKey( keys ), WobPlugin.ERROR );
                }
            } else {
                WobPlugin.Log( "ERROR: Attempt to set value for null key", WobPlugin.ERROR );
            }
        }
        /// <summary>
        /// Set the value of a config item, using the provided section and setting names.
        /// </summary>
        /// <typeparam name="T">The type of the value being set. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="section">The name of the section the config item is in.</param>
        /// <param name="name">The name of the config item.</param>
        /// <param name="newValue">The value to be written to the config item.</param>
        public static void Set<T>( string section, string name, T newValue ) { Set( new ConfigDefinition( section, name ), newValue ); }
        /// <summary>
        /// Set the value of a config item, using the default section name and a provided setting name.
        /// </summary>
        /// <typeparam name="T">The type of the value being set. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
        /// <param name="name">The name of the config item.</param>
        /// <param name="newValue">The value to be written to the config item.</param>
        public static void Set<T>( string name, T newValue ) { Set( new ConfigDefinition( DEFAULT_SECTION, name ), newValue ); }

        /// <summary>
        /// Safe type casts. It is safe to cast to the key value if the data type is in the return value list.
        /// </summary>
        private static readonly Dictionary<Type, HashSet<Type>> safeCasts = new Dictionary<Type, HashSet<Type>>() {
            // Integer types - can cast from any smaller integer type
            { typeof( short   ), new HashSet<Type> { typeof( sbyte ), typeof( byte ) } },
            { typeof( ushort  ), new HashSet<Type> { typeof( sbyte ), typeof( byte ) } },
            { typeof( int     ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ) } },
            { typeof( uint    ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ) } },
            { typeof( long    ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ) } },
            { typeof( ulong   ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ) } },
            // Floating point types - cast from integers or smaller floating points
            { typeof( float   ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ) } },
            { typeof( double  ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ) } },
            { typeof( decimal ), new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ) } },
        };

        /// <summary>
        /// Base class for configuration items, providing standard interactions and fields.
        /// </summary>
        public abstract class Entry {
            /// <summary>
            /// Object containing the section and setting names of the config item.
            /// </summary>
            protected ConfigDefinition Keys { get; private set; }
            /// <summary>
            /// Get the combination of section and setting names fomatted in the standard manner.
            /// </summary>
            public string FullKey {
                get {
                    return GetFullKey( this.Keys );
                }
            }
            /// <summary>
            /// The file that the config item is in.
            /// </summary>
            protected ConfigFile ConfigFile { get; private set; }
            /// <summary>
            /// The underlying config item in the file.
            /// </summary>
            protected ConfigEntryBase ConfigEntry { get; set; } // The value of this MUST be set in the constructor of the subclass
            /// <summary>
            /// The data type of this config item.
            /// </summary>
            public Type DataType {
                get {
                    return this.ConfigEntry.SettingType;
                }
            }

            /// <summary>
            /// Constructor that sets the key values for reading the item from the config file.
            /// </summary>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            protected Entry( ConfigFile file, ConfigDefinition keys ) {
                this.ConfigFile = file;
                this.Keys = keys;
            }

            /// <summary>
            /// Method to get the actual value of the setting. Override in sublass if something more complex is desired.
            /// </summary>
            /// <returns>The boxed value of the config item.</returns>
            protected virtual object GetValue() {
                return this.ConfigEntry.BoxedValue;
            }

            /// <summary>
            /// Read the value of a config item, with safe casting to the requested type.
            /// </summary>
            /// <typeparam name="T">The return type of the value. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
            /// <param name="defaultValue">The value to be returned in case of any errors reading the value from the config item.</param>
            /// <returns>The value read from the config file, or the default value parameter.</returns>
            public T Get<T>( T defaultValue ) {
                if( this.ConfigEntry == null ) {
                    WobPlugin.Log( "ERROR: Underlying config enrty not created for " + this.FullKey, WobPlugin.ERROR );
                    return defaultValue;
                } else {
                    T value = defaultValue;
                    // Check the data type being requested matches the target data type
                    if( typeof( T ) == this.DataType ) {
                        // Request and target are same data type, so return the value
                        value = (T)this.GetValue();
                    } else {
                        // Types don't match, so check the list of safe casts of numeric types
                        if( safeCasts.TryGetValue( typeof( T ), out HashSet<Type> safeCastList ) && safeCastList.Contains( this.DataType ) ) {
                            // This is a safe cast, so return the value
                            value = (T)Convert.ChangeType( this.GetValue(), typeof( T ) );
                        } else {
                            // Can't get the value, so log an error, and leave the return value as the default from the parameter
                            WobPlugin.Log( "ERROR: Attempt to get setting " + this.FullKey + " as " + typeof( T ) + " but it is " + this.DataType, WobPlugin.ERROR );
                        }
                    }
                    return value;
                }
            }

            /// <summary>
            /// Method to set the actual value of the setting. Override in sublass if additional restrictions apply.
            /// </summary>
            /// <param name="newValue">The value to be written to the config item.</param>
            protected virtual void SetValue( object newValue ) {
                this.ConfigEntry.BoxedValue = newValue;
            }

            /// <summary>
            /// Set the value of a config item, with safe casting from the provided type.
            /// </summary>
            /// <typeparam name="T">The type of the value being set. It will be safely cast to a larger ranged type if not a perfect match for the underlying type.</typeparam>
            /// <param name="newValue">The value to be written to the config item.</param>
            public void Set<T>( T newValue ) {
                if( this.ConfigEntry == null ) {
                    WobPlugin.Log( "ERROR: Underlying config enrty not created for " + this.FullKey, WobPlugin.ERROR );
                } else {
                    if( typeof( T ) == this.DataType ) {
                        // Request and target are same data type, so set the value
                        this.SetValue( newValue );
                    } else {
                        // Types don't match, so check the list of safe casts of numeric types
                        if( safeCasts.TryGetValue( this.DataType, out HashSet<Type> safeCastList ) && safeCastList.Contains( typeof( T ) ) ) {
                            // This is a safe cast, so return the value
                            this.SetValue( Convert.ChangeType( newValue, this.DataType ) );
                        } else {
                            // Can't get the value, so log an error, and leave the return value as the default from the parameter
                            WobPlugin.Log( "ERROR: Attempt to set setting " + this.FullKey + " as " + typeof( T ) + " but it is " + this.DataType, WobPlugin.ERROR );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Setting of boolean type.
        /// </summary>
        public class Boolean : Entry {
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( ConfigFile file, ConfigDefinition keys, string description, bool value ) : base( file, keys ) {
                this.ConfigEntry = file.Bind( keys, value, new ConfigDescription( description, new AcceptableValueList<bool>( new bool[] { true, false } ) ) );
            }
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( ConfigFile file, string section, string name, string description, bool value ) : this( file, new ConfigDefinition( section, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( ConfigFile file, string name, string description, bool value ) : this( file, new ConfigDefinition( DEFAULT_SECTION, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( ConfigDefinition keys, string description, bool value ) : this( WobPlugin.Config, keys, description, value ) { }
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( string section, string name, string description, bool value ) : this( WobPlugin.Config, new ConfigDefinition( section, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of boolean type.
            /// </summary>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Boolean( string name, string description, bool value ) : this( WobPlugin.Config, new ConfigDefinition( DEFAULT_SECTION, name ), description, value ) { }
        }

        /// <summary>
        /// Setting that uses an enum as the type. The config file uses the name of the enum values as strings for the value of the setting, and auto-validates against enum value list on read.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Enum<T> : Entry where T : Enum {
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( ConfigFile file, ConfigDefinition keys, string description, T value ) : base( file, keys ) {
                this.ConfigEntry = file.Bind( keys, value, new ConfigDescription( description ) );
            }
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( ConfigFile file, string section, string name, string description, T value ) : this( file, new ConfigDefinition( section, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( ConfigFile file, string name, string description, T value ) : this( file, new ConfigDefinition( DEFAULT_SECTION, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( ConfigDefinition keys, string description, T value ) : this( WobPlugin.Config, keys, description, value ) { }
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( string section, string name, string description, T value ) : this( WobPlugin.Config, new ConfigDefinition( section, name ), description, value ) { }
            /// <summary>
            /// Initialise a config item of enum type.
            /// </summary>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            public Enum( string name, string description, T value ) : this( WobPlugin.Config, new ConfigDefinition( DEFAULT_SECTION, name ), description, value ) { }
        }

        /// <summary>
        /// Setting of string type.
        /// </summary>
        public class String : Entry {
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( ConfigFile file, ConfigDefinition keys, string description, string value, string[] acceptedValues = null ) : base( file, keys ) {
                this.ConfigEntry = WobPlugin.Config.Bind( keys, value, new ConfigDescription( description, this.GetAcceptable( acceptedValues ) ) );
            }
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( ConfigFile file, string section, string name, string description, string value, string[] acceptedValues = null ) : this( file, new ConfigDefinition( section, name ), description, value, acceptedValues ) { }
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( ConfigFile file, string name, string description, string value, string[] acceptedValues = null ) : this( file, new ConfigDefinition( DEFAULT_SECTION, name ), description, value, acceptedValues ) { }
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( ConfigDefinition keys, string description, string value, string[] acceptedValues = null ) : this( WobPlugin.Config, keys, description, value, acceptedValues ) { }
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( string section, string name, string description, string value, string[] acceptedValues = null ) : this( WobPlugin.Config, new ConfigDefinition( section, name ), description, value, acceptedValues ) { }
            /// <summary>
            /// Initialise a config item of string type.
            /// </summary>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            public String( string name, string description, string value, string[] acceptedValues = null ) : this( WobPlugin.Config, new ConfigDefinition( DEFAULT_SECTION, name ), description, value, acceptedValues ) { }

            /// <summary>
            /// Get the object defining restrictions to the value that the config reader should enforce.
            /// </summary>
            /// <param name="acceptedValues">Specific values restriction. Value of <see langword="null"/> for no restriction.</param>
            /// <returns>Object containing the config reader enforced restrictions, or <see langword="null"/> for no restrictions.</returns>
            protected AcceptableValueBase GetAcceptable( string[] acceptedValues ) {
                if( acceptedValues != null ) {
                    return new AcceptableValueList<string>( acceptedValues );
                } else {
                    WobPlugin.Log( "WARNING: No validation for " + this.FullKey );
                    return null;
                }
            }
        }

        /// <summary>
        /// Setting of numeric type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Num<T> : Entry where T : IComparable, IEquatable<T> {
            /// <summary>
            /// Function to apply additional limits to a given value, such as rounding.
            /// </summary>
            protected Func<T, T> Limiter { get; set; }
            /// <summary>
            /// Multiply the value read from the config file by this number for use in patches.
            /// </summary>
            public float Scaler { get; protected set; }

            /// <summary>
            /// List of numeric primitive types. These are the types that T will be validated against.
            /// </summary>
            protected static readonly HashSet<Type> numericTypes = new HashSet<Type> { typeof( sbyte ), typeof( byte ), typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) };

            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( ConfigFile file, ConfigDefinition keys, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : base( file, keys ) {
                if( !numericTypes.Contains( typeof( T ) ) ) {
                    string message = "ERROR: Number config entry being created for non-numeric type " + typeof( T );
                    WobPlugin.Log( message, WobPlugin.ERROR );
                    throw new ArgumentException( message );
                }
                this.Limiter = limiter;
                this.Scaler = scaler;
                this.ConfigEntry = file.Bind( keys, value, new ConfigDescription( description, this.GetAcceptable( acceptedValues, bounds ) ) );
                if( this.Limiter != null ) {
                    this.SetValue( this.ConfigEntry.BoxedValue );
                }
            }
            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( ConfigFile file, string section, string name, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( file, new ConfigDefinition( section, name ), description, value, scaler, acceptedValues, bounds, limiter ) { }
            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="file">The file that the setting is saved in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( ConfigFile file, string name, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( file, new ConfigDefinition( DEFAULT_SECTION, name ), description, value, scaler, acceptedValues, bounds, limiter ) { }
            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="keys">Object defining the section and setting names for the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( ConfigDefinition keys, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( WobPlugin.Config, keys, description, value, scaler, acceptedValues, bounds, limiter ) { }
            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="section">The name of the section the config item is in.</param>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( string section, string name, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( WobPlugin.Config, new ConfigDefinition( section, name ), description, value, scaler, acceptedValues, bounds, limiter ) { }
            /// <summary>
            /// Initialise a config item of numeric type.
            /// </summary>
            /// <param name="name">The name of the config item.</param>
            /// <param name="description">Descriptive text to explain how the end-user should set the config item.</param>
            /// <param name="value">Initial value of the config item.</param>
            /// <param name="scaler">Multiply the value read from the config file by this number for use in patches.</param>
            /// <param name="acceptedValues">Restrict user input to this list of specific values. Use <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Restrict user input to this range of values. Use default or other value with equal min and max for no restriction.</param>
            /// <param name="limiter">Function to apply additional limits to a given value, such as rounding. Use <see langword="null"/> for no restriction.</param>
            /// <exception cref="ArgumentException">Throws exception if the type parameter is not a numeric primitive type.</exception>
            public Num( string name, string description, T value, float scaler = 0f, T[] acceptedValues = null, (T min, T max) bounds = default, Func<T, T> limiter = null ) : this( WobPlugin.Config, new ConfigDefinition( DEFAULT_SECTION, name ), description, value, scaler, acceptedValues, bounds, limiter ) { }

            /// <summary>
            /// Get the object defining restrictions to the value that the config reader should enforce.
            /// </summary>
            /// <param name="acceptedValues">Specific values restriction. Value of <see langword="null"/> for no restriction.</param>
            /// <param name="bounds">Range restriction. Value with equal min and max for no restriction.</param>
            /// <returns>Object containing the config reader enforced restrictions, or <see langword="null"/> for no restrictions.</returns>
            protected AcceptableValueBase GetAcceptable( T[] acceptedValues, (T min, T max) bounds ) {
                AcceptableValueBase acceptableValues = null;
                if( acceptedValues != null ) {
                    acceptableValues = new AcceptableValueList<T>( acceptedValues );
                } else {
                    if( bounds.min.CompareTo( bounds.max ) != 0 ) {
                        if( bounds.min.CompareTo( bounds.max ) < 0 ) {
                            acceptableValues = new AcceptableValueRange<T>( bounds.min, bounds.max );
                        } else {
                            acceptableValues = new AcceptableValueRange<T>( bounds.max, bounds.min );
                        }
                    } else {
                        WobPlugin.Log( "WARNING: No validation for " + this.FullKey );
                    }
                }
                return acceptableValues;
            }

            /// <summary>
            /// Override to the default method that multiplies the read value by the scaler if one has been set.
            /// </summary>
            /// <returns>The boxed value of the config item.</returns>
            protected override object GetValue() {
                if( this.Scaler == 0f ) {
                    return this.ConfigEntry.BoxedValue;
                }
                if( typeof( T ) == typeof( decimal ) ) {
                    return ( (decimal)this.Scaler ) * ( (decimal)Convert.ChangeType( this.ConfigEntry.BoxedValue, typeof( decimal ) ) );
                }
                if( typeof( T ) == typeof( double ) ) {
                    return ( (double)this.Scaler ) * ( (double)Convert.ChangeType( this.ConfigEntry.BoxedValue, typeof( double ) ) );
                }
                if( typeof( T ) == typeof( float ) || safeCasts[typeof( float )].Contains( typeof( T ) ) ) {
                    return this.Scaler * (float)Convert.ChangeType( this.ConfigEntry.BoxedValue, typeof( float ) );
                }
                WobPlugin.Log( "ERROR: Could not multiply scaler by type " + typeof( T ), WobPlugin.ERROR );
                return this.ConfigEntry.BoxedValue;
            }

            /// <summary>
            /// Override to default method that applies the limiter to the value before writing to the file.
            /// </summary>
            /// <param name="newValue">The value to be written to the config item.</param>
            protected override void SetValue( object newValue ) {
                if( this.Limiter != null ) {
                    this.ConfigEntry.BoxedValue = this.Limiter( (T)newValue );
                } else {
                    this.ConfigEntry.BoxedValue = newValue;
                }
            }
        }

        /// <summary>
        /// Class to help with using a type for setting lookup that has different names to those in the config files, so that the config items can use more user-friendly names from the UI.
        /// </summary>
        /// <typeparam name="T">Internal type used for lookups. Usually enum type or string.</typeparam>
        public class KeyHelper<T> {
            /// <summary>
            /// Set of translations from internal type to user-friendly name used in the config file.
            /// </summary>
            private readonly Dictionary<T, (string Section, string StatPrefix)> configNames = new Dictionary<T, (string Section, string StatPrefix)>();
            /// <summary>
            /// Prefix added to all section names, to keep them grouped in the file as it sorts sections alphabetically.
            /// </summary>
            private readonly string sectionPrefix;
            /// <summary>
            /// For sections with a specific order, add the index into the section name, zero-extended to this many digits.
            /// </summary>
            private readonly int digits;

            /// <summary>
            /// Constructor that sets the section prefix and optionally registers a set of translations.
            /// </summary>
            /// <param name="sectionPrefix">Prefix to be added to all section names, to keep them grouped in the file as it sorts sections alphabetically.</param>
            /// <param name="digits">For sections with a specific order, add the index into the section name, zero-extended to this many digits.</param>
            /// <param name="translations">Dictionary of internal types and their respective section names without prefix.</param>
            public KeyHelper( string sectionPrefix = null, int digits = 0, Dictionary<T, string> translations = null ) {
                this.sectionPrefix = sectionPrefix;
                this.digits = digits;
                this.Add( translations );
            }

            private string GetSectionPrefix( string prefixOverride ) {
                string sectionPrefix = "";
                if( prefixOverride != null ) {
                    sectionPrefix += prefixOverride;
                } else {
                    if( this.sectionPrefix != null ) { sectionPrefix += this.sectionPrefix; }
                }
                if( this.digits > 0 ) { sectionPrefix += ( this.configNames.Count + 1 ).ToString( "D" + this.digits ); }
                if( sectionPrefix != "" ) { sectionPrefix += "_"; }
                return sectionPrefix;
            }

            /// <summary>
            /// Register a new internal type to config key translation.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <param name="configKey">Name of the section without prefix.</param>
            public void Add( T internalType, string configKey ) { this.Add( internalType, null, configKey ); }
            /// <summary>
            /// Register a new internal type to config key translation.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <param name="sectionPrefix">Override for section the prefix defined in the constructor.</param>
            /// <param name="configKey">Name of the section without prefix.</param>
            public void Add( T internalType, string sectionPrefix, string configKey ) {
                if( this.configNames.TryGetValue( internalType, out (string Section, string StatPrefix) configKey2 ) ) {
                    WobPlugin.Log( "ERROR: Attempt to register " + internalType + " as " + configKey + " but it is already defined as " + configKey2.Section + "." + configKey2.StatPrefix );
                } else {
                    this.configNames.Add( internalType, (this.GetSectionPrefix( sectionPrefix ) + configKey, configKey + "_") );
                }
            }

            /// <summary>
            /// Register a set of new internal type to config key translations.
            /// </summary>
            /// <param name="translations">Dictionary of internal types and their respective section names without prefix.</param>
            public void Add( Dictionary<T, string> translations ) { this.Add( null, translations ); }
            /// <summary>
            /// Register a set of new internal type to config key translations.
            /// </summary>
            /// <param name="sectionPrefix">Override for section the prefix defined in the constructor.</param>
            /// <param name="translations">Dictionary of internal types and their respective section names without prefix.</param>
            public void Add( string sectionPrefix, Dictionary<T, string> translations ) {
                if( translations != null ) {
                    foreach( KeyValuePair<T, string> pair in translations ) {
                        this.Add( pair.Key, sectionPrefix, pair.Value );
                    }
                }
            }

            /// <summary>
            /// Register a new internal type to config key translation and return the keys.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <param name="configKey">Name of the section without prefix.</param>
            /// <param name="statName">Name of the stat without prefix.</param>
            /// <returns>Object containing the constructed names.</returns>
            public ConfigDefinition New( T internalType, string configKey, string statName ) { return this.New( internalType, null, configKey, statName ); }
            /// <summary>
            /// Register a new internal type to config key translation and return the keys.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <param name="sectionPrefix">Override for section the prefix defined in the constructor.</param>
            /// <param name="configKey">Name of the section without prefix.</param>
            /// <param name="statName">Name of the stat without prefix.</param>
            /// <returns>Object containing the constructed names.</returns>
            public ConfigDefinition New( T internalType, string sectionPrefix, string configKey, string statName ) {
                this.Add( internalType, sectionPrefix, configKey );
                return this.Get( internalType, statName );
            }

            /// <summary>
            /// Get the keys looked up for the given internal type.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <param name="statName">Name of the stat without prefix.</param>
            /// <returns>Object containing the constructed names.</returns>
            public ConfigDefinition Get( T internalType, string statName ) {
                if( this.configNames.TryGetValue( internalType, out (string Section, string StatPrefix) configKey ) ) {
                    return new ConfigDefinition( configKey.Section, configKey.StatPrefix + statName );
                } else {
                    return null;
                }
            }

            /// <summary>
            /// Check if there is a registered config key for the given internal type.
            /// </summary>
            /// <param name="internalType">Internal type to use for config key lookup.</param>
            /// <returns>Returns <see langword="true"/> if the type has a matching config key, otherwise <see langword="false"/>.</returns>
            public bool Exists( T internalType ) {
                return this.configNames.ContainsKey( internalType );
            }
        }

        /// <summary>
        /// Class to help with using a type for file lookup for when using multiple config files.
        /// </summary>
        /// <typeparam name="T">Internal type used for lookups. Usually enum type or string.</typeparam>
        public class FileHelper<T> {
            /// <summary>
            /// Set of translations from internal type to user-friendly name used in the config file.
            /// </summary>
            private readonly Dictionary<T, (string Name, ConfigFile File)> configNames = new Dictionary<T, (string Name, ConfigFile File)>();
            /// <summary>
            /// Prefix added to all section names, to keep them grouped in the file as it sorts sections alphabetically.
            /// </summary>
            private readonly string namePrefix;
            /// <summary>
            /// For files with a specific order, add the index into the file name, zero-extended to this many digits.
            /// </summary>
            private readonly int digits;

            /// <summary>
            /// Constructor that optionally registers a set of translations.
            /// </summary>
            /// <param name="namePrefix">Prefix to be added after GUID and before all name suffixes.</param>
            /// <param name="digits">For files with a specific order, add the index into the file name, zero-extended to this many digits.</param>
            /// <param name="translations">Dictionary of internal types and their respective file names without prefix.</param>
            public FileHelper( string namePrefix = null, int digits = 0, Dictionary<T, string> translations = null ) {
                this.namePrefix = namePrefix;
                this.digits = digits;
                this.Add( translations );
            }

            private string GetPathName( string nameSuffix, int index ) {
                string path = Paths.ConfigPath + "\\" + WobPlugin.Info.Metadata.GUID + ".";
                if( this.namePrefix != null ) { path += this.namePrefix; }
                if( this.digits > 0 ) { path += index.ToString( "D" + this.digits ); }
                if( this.namePrefix != null || this.digits > 0 ) { path += "_"; }
                path += nameSuffix + ".cfg";
                return path;
            }

            /// <summary>
            /// Register a new internal type to config flie translation.
            /// </summary>
            /// <param name="internalType">Internal type to use for config file lookup.</param>
            /// <param name="nameSuffix">Name of the file without prefix.</param>
            public void Add( T internalType, string nameSuffix ) {
                if( this.configNames.TryGetValue( internalType, out (string Name, ConfigFile File) configFile2 ) ) {
                    if( nameSuffix != configFile2.Name ) {
                        WobPlugin.Log( "ERROR: Attempt to register " + internalType + " as " + nameSuffix + " but it is already defined as " + configFile2.Name );
                    }
                } else {
                    this.configNames.Add( internalType, (nameSuffix, new ConfigFile( this.GetPathName( nameSuffix, this.configNames.Count + 1 ), true, WobPlugin.Info.Metadata ) ) );
                }
            }

            /// <summary>
            /// Register a set of new internal type to config file translations.
            /// </summary>
            /// <param name="translations">Dictionary of internal types and their respective file names without prefix.</param>
            public void Add( Dictionary<T, string> translations ) {
                if( translations != null ) {
                    foreach( KeyValuePair<T, string> pair in translations ) {
                        this.Add( pair.Key, pair.Value );
                    }
                }
            }

            /// <summary>
            /// Register a new internal type to config file translation and return the file.
            /// </summary>
            /// <param name="internalType">Internal type to use for config file lookup.</param>
            /// <param name="nameSuffix">Name of the file without prefix.</param>
            /// <returns>Reference to the file.</returns>
            public ConfigFile New( T internalType, string nameSuffix ) {
                this.Add( internalType, nameSuffix );
                return this.Get( internalType );
            }

            /// <summary>
            /// Get the file looked up for the given internal type.
            /// </summary>
            /// <param name="internalType">Internal type to use for config file lookup.</param>
            /// <returns>Reference to the file.</returns>
            public ConfigFile Get( T internalType ) {
                if( this.configNames.TryGetValue( internalType, out (string Name, ConfigFile File) configFile ) ) {
                    return configFile.File;
                } else {
                    return null;
                }
            }

            /// <summary>
            /// Check if there is a registered config file for the given internal type.
            /// </summary>
            /// <param name="internalType">Internal type to use for config file lookup.</param>
            /// <returns>Returns <see langword="true"/> if the type has a matching config file, otherwise <see langword="false"/>.</returns>
            public bool Exists( T internalType ) {
                return this.configNames.ContainsKey( internalType );
            }
        }
    }
}

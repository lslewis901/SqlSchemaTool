using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lewis.SST.DTSPackageClass
{
    #region DTSColumn class used in Column Collection
    /// <summary>
    /// Class to instantiate a DTS.Column object
    /// </summary>
    public class DTSColumn : DTS.Column
    {
        private enum colType
        {
            source = 0,
            destination
        }

        private int iNumericScale;
        private int iType;
        private int iDataType;
        private int iPrecision;
        private int iOrdinal;
        private int iFlags;
        private int iSize;

        private bool bNullable;
        private string sName;
        private object oColumnID;

        #region Column Members

        /// <summary>
        /// Gets or sets the Column type.
        /// </summary>
        /// <value>The type.</value>
        public int Type
        {
            get
            {
                return iType;
            }
            set
            {
                iType = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column numeric scale.
        /// </summary>
        /// <value>The numeric scale.</value>
        public int NumericScale
        {
            get
            {
                return iNumericScale;
            }
            set
            {
                iNumericScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the Column data.
        /// </summary>
        /// <value>The type of the data.</value>
        public int DataType
        {
            get
            {
                return iDataType;
            }
            set
            {
                iDataType = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column precision.
        /// </summary>
        /// <value>The precision.</value>
        public int Precision
        {
            get
            {
                return iPrecision;
            }
            set
            {
                iPrecision = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        public int Ordinal
        {
            get
            {
                return iOrdinal;
            }
            set
            {
                iOrdinal = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column flags.
        /// </summary>
        /// <value>The flags.</value>
        public int Flags
        {
            get
            {
                return iFlags;
            }
            set
            {
                iFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the column ID.
        /// </summary>
        /// <value>The column ID.</value>
        public object ColumnID
        {
            get
            {
                return oColumnID;
            }
            set
            {
                oColumnID = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column size.
        /// </summary>
        /// <value>The size.</value>
        public int Size
        {
            get
            {
                return iSize;
            }
            set
            {
                iSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the Column name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return sName;
            }
            set
            {
                sName = value;
            }
        }

        // this is just a placeholder for the implementation
        // we don't need it for the class object as we are using it
        /// <summary>
        /// Gets the DTS Column properties.
        /// </summary>
        /// <value>The properties.</value>
        public DTS.Properties Properties
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DTSColumn"/> is nullable.
        /// </summary>
        /// <value><c>true</c> if nullable; otherwise, <c>false</c>.</value>
        public bool Nullable
        {
            get
            {
                return bNullable;
            }
            set
            {
                bNullable = value;
            }
        }

        // this is just a placeholder for the implementation
        // we don't need it for the class object as we are using it
        /// <summary>
        /// Gets the Column parent object.
        /// </summary>
        /// <value>The parent.</value>
        public DTS.IDTSStdObject Parent
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
    #endregion

    #region DTSColumns Collection class
    /// <summary>
    /// Class to hold a collection of DTSColumn objects
    /// </summary>
    public class DTSColumns : CollectionBase
    {
        /// <summary>
        /// Gets or sets the <see cref="DTSColumn"/> at the specified index.
        /// </summary>
        /// <value>The DTSColumn value.</value>
        public DTSColumn this[int index]
        {
            get
            {
                return ((DTSColumn)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public int Add(DTSColumn value)
        {
            return (List.Add(value));
        }

        /// <summary>
        /// The Index of the DTS Column object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns the index of the DTS Column</returns>
        public int IndexOf(DTSColumn value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Inserts a DTS Column at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void Insert(int index, DTSColumn value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Removes the specified DTS Column value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Remove(DTSColumn value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Determines whether the collection [contains] [the specified DTS Column value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [contains] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(DTSColumn value)
        {
            return (List.Contains(value));
        }

        /// <summary>
        /// Performs additional custom processes before inserting a new element into the
        /// <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert <paramref name="value"/>.</param>
        /// <param name="value">The new value of the element at <paramref name="index"/>.</param>
        protected override void OnInsert(int index, Object value)
        {
            if (value.GetType() != Type.GetType("DTSColumn"))
                throw new ArgumentException("value must be of type DTSColumn.", "value");
        }

        /// <summary>
        /// Performs additional custom processes when removing an element from the
        /// <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> can be found.</param>
        /// <param name="value">The value of the element to remove from <paramref name="index"/>.</param>
        protected override void OnRemove(int index, Object value)
        {
            if (value.GetType() != Type.GetType("DTSColumn"))
                throw new ArgumentException("value must be of type DTSColumn.", "value");
        }

        /// <summary>
        /// Performs additional custom processes before setting a value in the
        /// <see cref="T:System.Collections.CollectionBase"/> instance.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="oldValue"/> can be found.</param>
        /// <param name="oldValue">The value to replace with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The new value of the element at <paramref name="index"/>.</param>
        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            if (newValue.GetType() != Type.GetType("DTSColumn"))
                throw new ArgumentException("newValue must be of type DTSColumn.", "newValue");
        }

        /// <summary>
        /// Performs additional custom processes when validating a value.
        /// </summary>
        /// <param name="value">The object to validate.</param>
        protected override void OnValidate(Object value)
        {
            if (value.GetType() != Type.GetType("DTSColumn"))
                throw new ArgumentException("value must be of type DTSColumn.");
        }

    }
    #endregion
}

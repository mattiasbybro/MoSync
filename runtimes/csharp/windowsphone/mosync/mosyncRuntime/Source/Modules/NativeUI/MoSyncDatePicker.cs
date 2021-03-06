﻿/* Copyright (C) 2011 MoSync AB

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License,
version 2, as published by the Free Software Foundation.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
MA 02110-1301, USA.
*/
/**
 * @file MoSyncDatePicker.cs
 * @author Rata Gabriela, Filipas Ciprian
 *
 * @brief This represents the DatePicker implementation for the NativeUI
 *        component on Windows Phone 7, language C#
 *
 * @platform WP 7.1
 **/

using Microsoft.Phone.Controls;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MoSync
{
    namespace NativeUI
    {
        /**
         * @author: Ciprian Filipas, Gabriela Rata
         * @brief: Mosync DatePicker implementation.
         */
        public class DatePicker : WidgetBaseWindowsPhone
        {
            // Some default values.
            private const int _maxYear = 3000;
            private const int _minYear = 1600;

            // The native DatePiker object.
            protected Microsoft.Phone.Controls.DatePicker mDatePicker;

            // Max/Min Date values
            protected DateTime mMaxDate;
            protected DateTime mMinDate;

            // The URI String for the CustomDatePickerPage.xaml containing the navigation parameters.
            private DatePickerPageCustomUriString mUriString;

            public DatePicker()
            {
                // Initialization.
                mDatePicker = new Microsoft.Phone.Controls.DatePicker();
                mView = mDatePicker;
                mUriString = new DatePickerPageCustomUriString();

                mMaxDate = new DateTime(_maxYear, 12, 31);
                mMinDate = new DateTime(_minYear, 1, 1);

                mUriString.MaxDate = mMaxDate;
                mUriString.MinDate = mMinDate;

                mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);

                // The ValueChanged event handler. This is when the MoSync event is triggered.
                mDatePicker.ValueChanged += new EventHandler<DateTimeValueChangedEventArgs>(
                    delegate(object from, DateTimeValueChangedEventArgs args)
                    {
                        Memory eventData = new Memory(20);

                        const int MAWidgetEventData_eventType = 0;
                        const int MAWidgetEventData_widgetHandle = 4;
                        const int MAWidgetEventDate_value_dayOfMonth = 8;
                        const int MAWidgetEventDate_value_month = 12;
                        const int MAWidgetEventDate_value_year = 16;
                        eventData.WriteInt32(MAWidgetEventData_eventType, MoSync.Constants.MAW_EVENT_DATE_PICKER_VALUE_CHANGED);
                        eventData.WriteInt32(MAWidgetEventData_widgetHandle, mHandle);
                        eventData.WriteInt32(MAWidgetEventDate_value_dayOfMonth, mDatePicker.Value.Value.Day);
                        eventData.WriteInt32(MAWidgetEventDate_value_month, mDatePicker.Value.Value.Month);
                        eventData.WriteInt32(MAWidgetEventDate_value_year, mDatePicker.Value.Value.Year);

                        mRuntime.PostCustomEvent(MoSync.Constants.EVENT_TYPE_WIDGET, eventData);
                    });
            }

            /**
             * @author: Ciprian Filipas
             * @brief: The MAW_DATE_PICKER_MAX_DATE_YEAR property implementation
             */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MAX_DATE_YEAR)]
            public int MaxDateYear
            {
                set
                {
                    try
                    {
                        if (mMinDate > mMaxDate.AddYears(-1 * (mMaxDate.Year - value)) || value > _maxYear) throw new InvalidPropertyValueException();

                        mMaxDate = mMaxDate.AddYears(-1 * (mMaxDate.Year - value));
                        mUriString.MaxDate = mMaxDate;
                        ResetDate();
                        mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                    }
                    catch
                    {
                        throw new InvalidPropertyValueException();
                    }
                }
                get
                {
                    return mMaxDate.Year;
                }
            }

            /**
             * @author: Ciprian Filipas
             * @brief: The MAW_DATE_PICKER_MAX_DATE_MONTH property implementation
             */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MAX_DATE_MONTH)]
            public int MaxDateMonth
            {
                set
                {
                    if (value <= 12 && value >= 1)
                    {
                        if (mMinDate > mMaxDate.AddMonths(-1 * (mMaxDate.Month - value))) throw new InvalidPropertyValueException();

                        mMaxDate = mMaxDate.AddMonths(-1 * (mMaxDate.Month - value));
                        mUriString.MaxDate = mMaxDate;
                        ResetDate();
                        mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                    }
                    else throw new InvalidPropertyValueException();
                }
                get
                {
                    return mMaxDate.Month;
                }
            }

            /**
             * @author: Ciprian Filipas
             * @brief: The MAW_DATE_PICKER_MAX_DATE_DAY property implementation
             */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MAX_DATE_DAY)]
            public int MaxDateDay
            {
                set
                {
                    int month = mMaxDate.Month;
                    if (mMinDate > mMaxDate.AddDays(value - mMaxDate.Day)) throw new InvalidPropertyValueException();

                    // If the month have changed it means that the day value was not valid.
                    if (month != mMaxDate.AddDays(value - mMaxDate.Day).Month) throw new InvalidPropertyValueException();

                    mMaxDate = mMaxDate.AddDays(value - mMaxDate.Day);
                    mUriString.MaxDate = mMaxDate;
                    ResetDate();
                    mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                }
                get
                {
                    return mMaxDate.Day;
                }
            }

            /**
             * @author: Ciprian Filipas
             * @brief: The MAW_DATE_PICKER_MIN_DATE_YEAR property implementation
             */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MIN_DATE_YEAR)]
            public int MinDateYear
            {
                set
                {
                    if (mMinDate.AddYears(-1 * (mMinDate.Year - value)) > mMaxDate || value < _minYear) throw new InvalidPropertyValueException();
                    mMinDate = mMinDate.AddYears(-1 * (mMinDate.Year - value));

                    mUriString.MinDate = mMinDate;
                    ResetDate();
                    mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                }
                get
                {
                    return mMinDate.Year;
                }
            }

            /**
            * @author: Ciprian Filipas
            * @brief: The MAW_DATE_PICKER_MIN_DATE_MONTH property implementation
            */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MIN_DATE_MONTH)]
            public int MinDateMonth
            {
                set
                {
                    if (value <= 12 && value >= 1)
                    {
                        if (mMinDate.AddMonths(-1 * (mMinDate.Month - value)) > mMaxDate) throw new InvalidPropertyValueException();

                        mMinDate = mMinDate.AddMonths(-1 * (mMinDate.Month - value));
                        mUriString.MinDate = mMinDate;
                        ResetDate();
                        mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                    }
                    else throw new InvalidPropertyValueException();
                }
                get
                {
                    return mMinDate.Month;
                }
            }

            /**
            * @author: Ciprian Filipas
            * @brief: The MAW_DATE_PICKER_MIN_DATE_DAY property implementation
            */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MIN_DATE_DAY)]
            public int MinDateDay
            {
                set
                {
                    int month = mMinDate.Month;
                    if (mMinDate.AddDays(value - mMinDate.Day) > mMaxDate) throw new InvalidPropertyValueException();

                    // IF the month have changed it means that the day value was not valid.
                    if (month != mMinDate.AddDays(value - mMinDate.Day).Month) throw new InvalidPropertyValueException();

                    mMinDate = mMinDate.AddDays(value - mMinDate.Day);
                    mUriString.MinDate = mMinDate;
                    ResetDate();
                    mDatePicker.PickerPageUri = new Uri(mUriString.UriString, UriKind.Relative);
                }
                get
                {
                    return mMinDate.Day;
                }
            }

            /**
            * @author: Ciprian Filipas, Gabriela Rata
            * @brief: The MAW_DATE_PICKER_YEAR property implementation
            */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_YEAR)]
            public int Year
            {
                set
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        if (mDatePicker.Value.Value.AddYears(-1 * (mDatePicker.Value.Value.Year - value)) > mMaxDate ||
                            mDatePicker.Value.Value.AddYears(-1 * (mDatePicker.Value.Value.Year - value)) < mMinDate)
                        {
                            throw new InvalidPropertyValueException();
                        }
                        else mDatePicker.Value = mDatePicker.Value.Value.AddYears(-1 * (mDatePicker.Value.Value.Year - value));
                    }
                }

                get
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        return myVal.GetValueOrDefault().Year;
                    }
                    return 0;
                }
            }//end Year

            /**
            * @author: Ciprian Filipas, Gabriela Rata
            * @brief: The MAW_DATE_PICKER_MONTH property implementation
            */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_MONTH)]
            public int Month
            {
                set
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        int year = mDatePicker.Value.Value.Year;

                        if (mDatePicker.Value.Value.AddMonths(-1 * (mDatePicker.Value.Value.Month - value)) > mMaxDate ||
                           mDatePicker.Value.Value.AddMonths(-1 * (mDatePicker.Value.Value.Month - value)) < mMinDate)
                        {
                            throw new InvalidPropertyValueException();
                        }

                        // If the year have changed it means that the month value was not valid.
                        if (year != mDatePicker.Value.Value.AddMonths(-1 * (mDatePicker.Value.Value.Month - value)).Year) throw new InvalidPropertyValueException();

                        mDatePicker.Value = mDatePicker.Value.Value.AddMonths(-1 * (mDatePicker.Value.Value.Month - value));
                    }
                }

                get
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        return myVal.GetValueOrDefault().Month;
                    }
                    return 0;
                }
            }//end Month

           /**
           * @author: Ciprian Filipas, Gabriela Rata
           * @brief: The MAW_DATE_PICKER_DAY_OF_MONTH property implementation
           */
            [MoSyncWidgetProperty(MoSync.Constants.MAW_DATE_PICKER_DAY_OF_MONTH)]
            public int dayOfMonth
            {
                set
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        int month = mDatePicker.Value.Value.Month;

                        if (mDatePicker.Value.Value.AddDays(-1 * (mDatePicker.Value.Value.Day - value)) > mMaxDate ||
                            mDatePicker.Value.Value.AddDays(-1 * (mDatePicker.Value.Value.Day - value)) < mMinDate)
                        {
                            throw new InvalidPropertyValueException();
                        }

                        //if the month have changed it means that the day value was not valid.
                        if (month != mDatePicker.Value.Value.AddDays(-1 * (mDatePicker.Value.Value.Day - value)).Month) throw new InvalidPropertyValueException();

                        mDatePicker.Value = mDatePicker.Value.Value.AddDays(-1 * (mDatePicker.Value.Value.Day - value));
                    }
                }

                get
                {
                    System.DateTime? myVal = mDatePicker.Value;
                    if (myVal.HasValue)
                    {
                        return myVal.GetValueOrDefault().Day;
                    }
                    return 0;
                }
            }//end Day

            /**
           * @author: Ciprian Filipas
           * @brief: if the DatePicker value is out of bounds we reset it to the closest value, Min or Max.
           */
            private void ResetDate()
            {
                if (mDatePicker.Value > mMaxDate) mDatePicker.Value = mMaxDate;
                else if (mDatePicker.Value < mMinDate) mDatePicker.Value = mMinDate;
            }
        }//end DatePicker

    } //end NativeUI
}//end MoSync
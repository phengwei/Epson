<template>
  <Datepicker :disabled-dates="disabledDatesObject" inline></Datepicker>
</template>

<script>
  import Datepicker from 'vuejs-datepicker';

  export default {
    components: {
      Datepicker
    },
    props: {
      existingHolidays: {
        type: Array,
        required: true
      },
      isEditable: {
        type: Boolean,
        default: true
      }
    },
    computed: {
      disabledDatesObject() {
        const dates = this.existingHolidays.map(date => new Date(date));
        return { dates };
      }
    }
  }
</script>

<style>

  .datepicker {
    margin-bottom: 1rem;
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    background-color: #fff;
  }

    .datepicker .day.disabled {
      background: #ff0;
    }
</style>

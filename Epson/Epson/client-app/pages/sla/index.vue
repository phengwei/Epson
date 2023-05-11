<template>
  <div class="sla-management" v-if="loggedInUser.roles.includes('Admin')">
    <h1>SLA Management</h1>

    <form>
      <div class="checkbox-group">
        <label>
          <input type="checkbox" v-model="workingHours">
          Include working hours
        </label>

        <div class="working-hours" v-if="workingHours">
          <div>
            <label for="workingHoursStart">Start:</label>
            <input type="time" id="workingHoursStart" v-model="workingHoursStart">
            <label for="workingHoursEnd">End:</label>
            <input type="time" id="workingHoursEnd" v-model="workingHoursEnd">
          </div>
        </div>
      </div>

      <div class="checkbox-group">
        <label>
          <input type="checkbox" id="holidays" v-model="holidays">
          Include holidays
        </label>
      </div>

      <div class="checkbox-group">
        <label>
          <input type="checkbox" id="staffLeaves" v-model="staffLeaves">
          Include staff leaves
        </label>
      </div>

      <div class="deadline">
        <label for="deadlineHours">Deadline in hours:</label>
        <input type="number" id="deadlineHours" v-model="deadlineHours">
      </div>

      <div class="form-actions">
        <button type="button" @click="test">Save</button>
        <button type="button" @click="resetForm">Reset</button>
      </div>
    </form>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  export default {
    name: 'SLA',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        workingHours: false,
        workingHoursStart: null,
        workingHoursEnd: null,
        deadlineHours: null,
        holidays: false,
        staffLeaves: false,
      };
    },
    methods: {
      resetForm() {
        this.workingHours = false;
        this.workingHoursStart = null;
        this.workingHoursEnd = null;
        this.deadlineHours = null;
        this.holidays = false;
        this.staffLeaves = false;
      },
      test() {
        console.log(this);
      },
      async getSLASettings() {
        try {
          const response = await fetch('api/sla/getslasettings');

          const obj = await response.json();
          this.workingHours = obj.data.includeWorkingHours;
          this.workingHoursStart = `${obj.data.workingStartHour.toString().padStart(2, '0')}:${obj.data.workingStartMinute.toString().padStart(2, '0')}`;
          this.workingHoursEnd = `${obj.data.workingEndHour.toString().padStart(2, '0')}:${obj.data.workingEndMinute.toString().padStart(2, '0')}`;
          this.deadlineHours = obj.data.deadlineInHours;
          this.holidays = obj.data.includeHoliday;
          this.staffLeaves = obj.data.includeStaffLeaves;

        } catch (error) {
          console.error('There was a problem fetching the SLA settings:', error);
        }
      }
    },
    mounted() {
      this.getSLASettings();
    },
    watch: {
      workingHours(value) {
        if (!value) {
          this.workingHoursStart = null;
          this.workingHoursEnd = null;
        }
      }
    }
  }
</script>

<style scoped>
  .sla-management {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100vh;
  }

  form {
    max-width: 500px;
    padding: 2rem;
    background-color: #fff;
    border: 1px solid #ccc;
    box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.3);
  }

  h1 {
    margin-top: 0;
    font-size: 2rem;
    text-align: center;
  }

  .checkbox-group {
    margin-bottom: 1rem;
  }

  label {
    display: flex;
    align-items: center;
    font-weight: bold;
    margin-bottom: 0.5rem;
  }

  input[type="checkbox"] {
    margin-right: 0.5rem;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
    border-radius: 3px;
    border: 2px solid #ccc;
    width: 1.2em;
    height: 1.2em;
    margin-left: 5%
  }

    input[type="checkbox"]:checked {
      background-color: #4285f4;
      border-color: #4285f4;
    }

  input[type="time"],
  input[type="number"] {
    margin-left: 0.5rem;
    padding: 0.25rem;
    border: 1px solid #ccc;
    border-radius: 0.25rem;
  }

  .working-hours {
    margin-left: 2rem;
    display: flex;
    align-items: center;
  }

  .deadline {
    margin-left: 2rem;
    display: flex;
    align-items: center;
  }

  .form-actions {
    display: flex;
    justify-content: center;
    margin-top: 1rem;
  }

  button {
    margin: 0 0.5rem;
    padding: 0.5rem 1rem;
    font-size: 1rem;
    font-weight: bold;
    color: #fff;
    background-color: #4285f4;
    border: none;
    border-radius: 0.25rem;
    cursor: pointer;
    transition: background-color 0.3s;
  }

    button:hover {
      background-color: #3367d6;
    }

  @media (max-width: 768px) {
    form {
      max-width: 300px;
      padding: 1rem;
    }

    h1 {
      font-size: 1.5rem;
    }

    .working-hours,
    .deadline {
      margin-left: 0;
    }
  }
</style>

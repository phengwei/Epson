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

      <div class="form-actions">
        <button type="submit" @click="saveSLASettings">Save</button>
        <button type="button" @click="resetForm">Reset</button>
      </div>
    </form>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  export default {
    name: 'SLA-Settings',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        workingHours: false,
        workingHoursStart: null,
        workingHoursEnd: null,
        holidays: false,
        staffLeaves: false,
      };
    },
    methods: {
      resetForm() {
        this.workingHours = false;
        this.workingHoursStart = null;
        this.workingHoursEnd = null;
        this.holidays = false;
        this.staffLeaves = false;
      },
      async saveSLASettings() {
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/sla/updateslasettings`, {
            data: {
              IncludeHoliday: this.holidays,
              IncludeStaffLeaves: this.staffLeaves,
              IncludeWorkingHours: this.workingHours,
              WorkingStartHour: parseInt(this.workingHoursStart.split(':')[0]),
              WorkingStartMinute: parseInt(this.workingHoursStart.split(':')[1]),
              WorkingEndHour: parseInt(this.workingHoursEnd.split(':')[0]),
              WorkingEndMinute: parseInt(this.workingHoursEnd.split(':')[1])
            }
          }).then(response => {
          })
        } catch (error) {
          console.error('There was a problem updating SLA settings');
        }
      },
      async getSLASettings() {
        try {
          this.loading = true
          await this.$axios.get(`${this.$config.restUrl}/api/sla/getslasettings`).then(result => {
            this.workingHours = result.data.data.includeWorkingHours
            this.workingHoursStart = `${result.data.data.workingStartHour.toString().padStart(2, '0')}:${result.data.data.workingStartMinute.toString().padStart(2, '0')}`;
            this.workingHoursEnd = `${result.data.data.workingEndHour.toString().padStart(2, '0')}:${result.data.data.workingEndMinute.toString().padStart(2, '0')}`;
            this.holidays = result.data.data.includeHoliday
            this.staffLeaves = result.data.data.includeStaffLeaves
          })

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

<template>
  <nav class="ums-header bg-[#003399] top-0 inset-x-0 w-full z-30 text-white fixed transition duration-300 delay-0 ease-out">
    <div class="flex sm:px-18 px-4">
      <div class="w-full flex justify-between items-center">
        <!-- Website Logo -->
        <div class="flex items-center">
          <nuxt-link to="/" class="py-4 px-2 w-[150px]">
            <img class="w-[150px] h-14 object-contain"
                 src="/images/svg/epson-logo.png"
                 alt="Epson" />
          </nuxt-link>
        </div>

        <!-- Primary Navbar items -->
        <div class="flex items-center space-x-7">
          <nuxt-link to="/dashboard"
                     exact-active-class="nav-link-active"
                     class="nav-link hover:bg-[#19212b] px-3 py-2 font-semibold transition duration-300">Dashboard</nuxt-link>
          <nuxt-link to="/reporting"
                     exact-active-class="nav-link-active"
                     class="nav-link hover:bg-[#19212b] px-3 py-2 font-semibold transition duration-300">Reports</nuxt-link>
          <nuxt-link to="/slaDashboard"
                     exact-active-class="nav-link-active"
                     class="nav-link hover:bg-[#19212b] px-3 py-2 font-semibold transition duration-300">SLA Overview</nuxt-link>
          <nuxt-link to="/request"
                     exact-active-class="nav-link-active"
                     class="nav-link hover:bg-[#19212b] px-3 py-2 font-semibold transition duration-300">Requests</nuxt-link>
          <nuxt-link to="/product"
                     exact-active-class="nav-link-active"
                     class="nav-link hover:bg-[#19212b] px-3 py-2 font-semibold transition duration-300">Products</nuxt-link>
        </div>

        <!-- User Dropdown -->
        <div class="relative ml-auto" ref="dropdown">
          <span @click="toggleDropdown" class="px-3 py-2 font-semibold transition duration-300 cursor-pointer flex items-center italic">
            {{ loggedInUser.userName }}
            <span class="ml-4">&#x25BC;</span>
          </span>
          <div v-show="showDropdown" class="dropdown-menu absolute right-0 mt-1 w-48 rounded-md shadow-lg py-1 bg-white text-black z-50">
            <nuxt-link to="/change-password"
                       class="block px-4 py-2 hover:bg-[#003399] hover:text-white">Change Password</nuxt-link>
            <a class="block px-4 py-2 hover:bg-[#003399] hover:text-white cursor-pointer"
               @click="logout">Log Out</a>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>
<script>
  import { mapGetters } from 'vuex';

  export default {
    name: 'HeaderNav',
    data() {
      return {
        showDropdown: false
      };
    },
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
    },
    methods: {
      toggleDropdown() {
        this.showDropdown = !this.showDropdown;
        console.log('Dropdown toggled:', this.showDropdown);
      },
      closeDropdown(event) {
        if (!this.$refs.dropdown.contains(event.target)) {
          this.showDropdown = false;
        }
      },
      logout() {
        this.$auth.logout().then(() => {
          localStorage.clear();
          this.$router.push('/login');
          this.$router.go(0);
        });
      }
    },
    mounted() {
      document.addEventListener('click', this.closeDropdown);
    },
    beforeDestroy() {
      document.removeEventListener('click', this.closeDropdown);
    }
  };
</script>
<style scoped>
  .ums-header {
    background-color: #003399;
  }

  .nav-link {
    position: relative;
  }

  .nav-link-active {
    color: #ffffff !important;
  }

    .nav-link-active::after {
      content: '';
      position: absolute;
      width: 100%;
      height: 2px;
      background-color: #ffffff;
      left: 0;
      bottom: -2px;
    }

  .dropdown-menu {
    display: block; /* Ensure the dropdown is shown */
  }

  .homeHeader {
    background-color: #f1f3f5;
    color: rgb(107 114 128);
  }

  .homePageMobile {
    background-color: #f1f3f5;
  }

  .homePageIsActive {
    color: #003399;
    border-color: #003399;
    border-bottom-width: 4px;
  }

  .homePageIsActiveScrolled {
    color: #003399;
    border-color: #003399;
  }

  .homePageIsActiveMobile {
    background-color: #003399;
    color: white;
  }

  .stop-scrolling {
    height: 100%;
    overflow: hidden
  }

  .transition-right {
    transform: translate(100%, 0);
  }
</style>

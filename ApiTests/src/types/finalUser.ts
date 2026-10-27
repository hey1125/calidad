export interface FinalUser {
  id: number;
  nationalId: string;
  firstName: string;
  lastName1: string;
  lastName2: string;
  passwordHash: string;
  phone: string;
  email: string;
  dateOfBirth: string;
  locationLat: number;
  locationLng: number;
  selfieUrl: string;
  facialVerificationPassed: boolean;
  profilePhotoUrl: string;
  idCardFrontPhotoUrl: string;
  idCardBackPhotoUrl: string;
  status: 'Pending' | 'Active' | 'Inactive';
  created: string;
  updated: string;
}
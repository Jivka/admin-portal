import { Box, LinearProgress, Typography, Stack } from '@mui/material';
import { CheckCircle, Cancel } from '@mui/icons-material';

interface PasswordStrengthProps {
  password: string;
}

interface PasswordRequirement {
  label: string;
  test: (password: string) => boolean;
}

const requirements: PasswordRequirement[] = [
  { label: 'At least 8 characters', test: (pwd) => pwd.length >= 8 },
  { label: 'At most 16 characters', test: (pwd) => pwd.length <= 16 },
  { label: 'Contains lowercase letter', test: (pwd) => /[a-z]/.test(pwd) },
  { label: 'Contains uppercase letter', test: (pwd) => /[A-Z]/.test(pwd) },
  { label: 'Contains digit', test: (pwd) => /\d/.test(pwd) },
  { label: 'Contains special character', test: (pwd) => /[^a-zA-Z0-9]/.test(pwd) },
];

/**
 * PasswordStrength component for real-time password validation
 * Validates against backend rules: 8-16 chars, lowercase, uppercase, digit, special char
 */
export const PasswordStrength = ({ password }: PasswordStrengthProps) => {
  // Don't show anything if password is empty
  if (!password) return null;

  // Calculate met requirements
  const metRequirements = requirements.filter((req) => req.test(password));
  const strengthPercentage = (metRequirements.length / requirements.length) * 100;

  // Determine color based on strength
  const getColor = () => {
    if (strengthPercentage < 50) return 'error';
    if (strengthPercentage < 100) return 'warning';
    return 'success';
  };

  const getStrengthLabel = () => {
    if (strengthPercentage < 50) return 'Weak';
    if (strengthPercentage < 100) return 'Medium';
    return 'Strong';
  };

  return (
    <Box sx={{ mt: 1, mb: 2 }}>
      {/* Strength indicator */}
      <Box sx={{ mb: 1 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
          <Typography variant="caption" color="text.secondary">
            Password Strength
          </Typography>
          <Typography variant="caption" color={`${getColor()}.main`} fontWeight={600}>
            {getStrengthLabel()}
          </Typography>
        </Box>
        <LinearProgress 
          variant="determinate" 
          value={strengthPercentage} 
          color={getColor()}
          sx={{ height: 6, borderRadius: 1 }}
        />
      </Box>

      {/* Requirements checklist */}
      <Stack spacing={0.5}>
        {requirements.map((req, index) => {
          const isMet = req.test(password);
          return (
            <Box 
              key={index} 
              sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
            >
              {isMet ? (
                <CheckCircle sx={{ fontSize: 16, color: 'success.main' }} />
              ) : (
                <Cancel sx={{ fontSize: 16, color: 'error.main' }} />
              )}
              <Typography 
                variant="caption" 
                color={isMet ? 'success.main' : 'text.secondary'}
              >
                {req.label}
              </Typography>
            </Box>
          );
        })}
      </Stack>
    </Box>
  );
};

export default PasswordStrength;
